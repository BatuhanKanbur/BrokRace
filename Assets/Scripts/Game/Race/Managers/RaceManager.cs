using System;
using System.Collections.Generic;
using Core.DI.Attributes;
using Core.Utilities;
using Cysharp.Threading.Tasks;
using Game.Ai.Interfaces;
using Game.Ai.Managers;
using Game.Balancing.Interfaces;
using Game.Balancing.Managers;
using Game.Boost.Enums;
using Game.Camera.Interfaces;
using Game.Cars.Interfaces;
using Game.Cars.Structure;
using Game.Configuration.Interfaces;
using Game.Configuration.Structure;
using Game.Input.Interfaces;
using Game.Race.Interfaces;
using Game.Race.Structure;
using Game.Standings.Interfaces;
using Game.Standings.Managers;
using Game.Telemetry.Interfaces;
using Game.Telemetry.Managers;
using Game.Telemetry.Structure;
using Game.Track.Interfaces;
using PoolManager.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static Game.Race.Constants.RaceConstants;
using Pool = PoolManager.Runtime.PoolManager;

namespace Game.Race.Managers
{
    public class RaceManager : MonoBehaviour, IRaceManager, IRaceState
    {
        [SerializeField] private Transform carRoot;

        [Inject] private IRaceConfigService _configService;
        [Inject] private ITrackBuilder _trackBuilder;
        [Inject] private IRaceCamera _camera;

        private readonly List<ICar> _cars = new();
        private readonly List<IAiDriver> _drivers = new();
        private readonly List<FinishCrossing> _crossings = new();
        private readonly StandingsManager _standings = new();

        private IRaceBalancer _balancer;
        private ITelemetryRecorder _telemetry;
        private IBoostInputSource _input;
        private RaceConfig _config;
        private float[] _laneOffsets;
        private float _accumulator;

        public event Action<ICarProgress> OnCarFinished;
        public event Action OnRaceCompleted;
        public event Action<int> OnBoostAccepted;
        public event Action<int, BoostRequestOutcome> OnBoostRejected;

        public int Seed { get; private set; }
        public float Time { get; private set; }
        public float RaceDistance => _config.track.raceDistance;
        public bool IsRunning { get; private set; }
        public IReadOnlyList<ICar> Cars => _cars;
        public ICar Player { get; private set; }
        public IRaceOrder Order => _standings;
        public ITelemetryRecorder Telemetry => _telemetry;

        public async UniTask Build()
        {
            _config = _configService.Config;
            await _trackBuilder.Build(_config.track);
            await SpawnGrid();
            _standings.Register(_cars);
            _balancer = new RubberBandBalancer(_config.balance, this);
            _camera.Follow(Player.Transform);
        }

        public void Prepare(int seed, IBoostInputSource input)
        {
            Seed = seed;
            Time = 0f;
            _accumulator = 0f;
            IsRunning = false;
            _crossings.Clear();
            _drivers.Clear();

            DetachInput();
            _input = input;
            _input.Reset();
            _input.OnBoostRequested += HandleRequest;

            for (var index = 0; index < _cars.Count; index++)
                _cars[index].Initialize(BuildSetup(index));
            for (var index = 1; index < _cars.Count; index++)
                _drivers.Add(new AiDriver(_cars[index], _config.rivals[index - 1].profile, _config.ai, _config.boost,
                    this, _standings, DeterministicRandom.Stream(seed, index)));

            _telemetry = new TelemetryRecorder(_config.telemetry, this, _standings, _drivers);
            foreach (var car in _cars)
            {
                var tracked = car;
                car.OnCrossedFinish -= HandleCrossing;
                car.OnCrossedFinish += HandleCrossing;
                car.Boost.OnBoostStarted += level => _telemetry.RecordRequest(tracked, level, BoostRequestOutcome.Accepted);
                car.Boost.OnRequestRejected += (level, outcome) => _telemetry.RecordRequest(tracked, level, outcome);
            }

            _standings.Reset();
            _balancer.Reset();
            _camera.Snap();
            Present(0f);
        }

        public void Begin()
        {
            IsRunning = true;
            foreach (var car in _cars)
                car.OpenBoostWindow();
            _telemetry.Begin(BuildRunInfo());
        }

        public void Tick(float frameTime)
        {
            if (!IsRunning) return;
            _accumulator += Mathf.Min(frameTime, _config.race.maxFrameTime);
            var step = Mathf.Max(_config.race.logicStep, MinimumLogicStep);
            while (_accumulator >= step && IsRunning)
            {
                Step(step);
                _accumulator -= step;
            }
        }

        public void Present(float deltaTime)
        {
            foreach (var car in _cars)
                car.Present(deltaTime);
            _camera.Present(deltaTime);
        }

        public void Conclude()
        {
            IsRunning = false;
            foreach (var car in _cars)
                car.CloseBoostWindow();
        }

        public RaceReport BuildReport(string scenario, int frameRateTarget)
        {
            var report = _telemetry.Complete();
            report.run.scenario = scenario;
            report.run.frameRateTarget = frameRateTarget;
            return report;
        }

        public void Teardown()
        {
            DetachInput();
            foreach (var car in _cars)
            {
                car.OnCrossedFinish -= HandleCrossing;
                car.Release();
            }
            _cars.Clear();
            _drivers.Clear();
        }

        private void Step(float stepTime)
        {
            var stepStart = Time;
            _input.Sample(stepStart);
            foreach (var driver in _drivers)
                driver.Step(stepTime);
            _balancer.Step(stepTime);
            foreach (var car in _cars)
                car.Step(stepTime);
            Time = stepStart + stepTime;
            _standings.Refresh();
            ResolveCrossings(stepStart);
            _telemetry.Step(stepTime);
        }

        private void ResolveCrossings(float stepStart)
        {
            if (_crossings.Count == 0) return;
            _crossings.Sort(CompareCrossings);
            foreach (var crossing in _crossings)
            {
                _standings.ReportFinish(crossing.Car, stepStart + crossing.Offset);
                _telemetry.RecordFinish(crossing.Car);
                OnCarFinished?.Invoke(crossing.Car);
            }
            _crossings.Clear();
            _standings.Refresh();
            if (_standings.FinishedCount < _cars.Count) return;
            Conclude();
            OnRaceCompleted?.Invoke();
        }

        private static int CompareCrossings(FinishCrossing left, FinishCrossing right)
        {
            var byTime = left.Offset.CompareTo(right.Offset);
            if (byTime != 0) return byTime;
            var bySpeed = right.Car.Speed.CompareTo(left.Car.Speed);
            return bySpeed != 0 ? bySpeed : left.Car.Index.CompareTo(right.Car.Index);
        }

        private void HandleCrossing(ICar car, float offset) => _crossings.Add(new FinishCrossing(car, offset));

        private void HandleRequest(int level)
        {
            var outcome = Player.Boost.Request(level);
            if (outcome == BoostRequestOutcome.Accepted) OnBoostAccepted?.Invoke(level);
            else OnBoostRejected?.Invoke(level, outcome);
        }

        private void DetachInput()
        {
            if (_input == null) return;
            _input.OnBoostRequested -= HandleRequest;
            _input = null;
        }

        private async UniTask SpawnGrid()
        {
            _cars.Add(await SpawnCar(_config.player.prefab));
            foreach (var rival in _config.rivals)
                _cars.Add(await SpawnCar(rival.prefab));
            Player = _cars[PlayerIndex];
            var lanes = BuildLaneOrder(_cars.Count);
            _laneOffsets = new float[_cars.Count];
            for (var index = 0; index < _cars.Count; index++)
                _laneOffsets[index] = (lanes[index] - (_cars.Count - 1) * 0.5f) * _config.track.laneSpacing;
        }

        private async UniTask<ICar> SpawnCar(AssetReference prefab)
        {
            var instance = await Pool.GetObjectAsync(prefab).SetParent(carRoot);
            return instance.GetComponent<ICar>();
        }

        private CarSetup BuildSetup(int index)
        {
            if (index == PlayerIndex)
                return new CarSetup(index, _config.player.displayName, true, _config.player.paint,
                    _laneOffsets[index], _config.race.baseSpeed, _config.track.raceDistance, _config.boost);
            var rival = _config.rivals[index - 1];
            return new CarSetup(index, rival.displayName, false, rival.paint, _laneOffsets[index],
                _config.race.baseSpeed * rival.profile.speedScale, _config.track.raceDistance, _config.boost);
        }

        private RaceRunInfo BuildRunInfo() => new()
        {
            seed = Seed,
            scenario = string.Empty,
            raceDistance = _config.track.raceDistance,
            baseSpeed = _config.race.baseSpeed,
            logicStep = _config.race.logicStep,
            boostWindow = _config.boost.windowDuration,
            balancingEnabled = _balancer.IsEnabled
        };

        private static int[] BuildLaneOrder(int count)
        {
            var lanes = new int[count];
            var centre = count / 2;
            lanes[PlayerIndex] = centre;
            var cursor = 0;
            for (var index = 1; index < count; index++)
            {
                if (cursor == centre) cursor++;
                lanes[index] = cursor++;
            }
            return lanes;
        }
    }
}
