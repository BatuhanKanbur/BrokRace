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
using Game.Boost.Logics;
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
        private readonly List<FinishCrossing> _finishing = new();
        private readonly List<int> _pendingRequests = new();
        private readonly StandingsManager _standings = new();

        private IRaceBalancer _balancer;
        private IAiAirspace _airspace;
        private TelemetryRecorder _telemetry;
        private IBoostInputSource _input;
        private RaceConfig _config;
        private float[] _laneOffsets;
        private int[] _slotToProfile;
        private float _accumulator;
        private int _stepIndex;

        public event Action<int> OnBoostAccepted;
        public event Action<int, BoostRequestOutcome> OnBoostRejected;

        public int Seed { get; private set; }
        public float Time { get; private set; }
        public float RaceDistance => _config.track.raceDistance;
        public float LogicStep => Mathf.Max(_config.race.logicStep, MinimumLogicStep);
        public bool IsRunning { get; private set; }
        public bool HasCompleted { get; private set; }
        public IReadOnlyList<ICar> Cars => _cars;
        public ICar Player { get; private set; }
        public IRaceOrder Order => _standings;
        public IRaceBalancer Balancer => _balancer;
        public ITelemetryLog Log => _telemetry;
        public IReadOnlyList<IAiDriver> Drivers => _drivers;

        public async UniTask Build()
        {
            _config = _configService.Config;
            await _trackBuilder.Build(_config.track);
            await SpawnGrid();
            _standings.Register(_cars);
            _standings.OnCarFinished += HandleCarFinished;
            _balancer = new RubberBandBalancer(_config.balance, this, _standings);
            _airspace = new AiAirspace(_config.ai.airspaceGate);
            _camera.Follow(Player.Transform);
        }

        public void Prepare(int seed, IBoostInputSource input)
        {
            Seed = seed;
            Time = 0f;
            _accumulator = 0f;
            _stepIndex = 0;
            IsRunning = false;
            HasCompleted = false;
            _crossings.Clear();
            _pendingRequests.Clear();
            _drivers.Clear();

            DetachInput();
            _input = input;
            _input.Reset();
            _input.OnBoostRequested += Enqueue;

            _slotToProfile = ShuffledProfiles(seed);
            for (var index = 0; index < _cars.Count; index++)
                _cars[index].Initialize(BuildSetup(index, seed));
            for (var index = 1; index < _cars.Count; index++)
                _drivers.Add(new AiDriver(_cars[index], ProfileFor(index), _config.ai, _config.boost, this,
                    _standings, LogicStep, seed));

            _telemetry = new TelemetryRecorder(_config.telemetry, this, _standings, _drivers);
            _standings.Reset();
            _balancer.Reset();
            _balancer.Register(Player, 0f);
            for (var index = 1; index < _cars.Count; index++)
                _balancer.Register(_cars[index], ProfileFor(index).balanceResponse);
            _airspace.Reset();
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

        public void PumpInput()
        {
            _input.Poll();
            _input.Sample(Time);
            DrainRequests();
        }

        public void Tick(float frameTime)
        {
            _input.Poll();
            if (!IsRunning) return;
            _accumulator += Mathf.Min(frameTime, _config.race.maxFrameTime);
            var step = LogicStep;
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
                car.Release();
            _cars.Clear();
            _drivers.Clear();
        }

        private void Step(float stepTime)
        {
            var stepStart = Time;
            _input.Sample(stepStart);
            DrainRequests();
            RunDrivers();
            _balancer.Step(stepTime);
            foreach (var car in _cars)
            {
                var motion = car.Step(stepTime);
                if (motion.HasCrossed && !car.HasFinished)
                    _crossings.Add(new FinishCrossing(car, motion.CrossOffset));
            }
            Time = stepStart + stepTime;
            _standings.Refresh();
            if (_crossings.Count > 0) ResolveCrossings(stepStart);
            _telemetry.Step(stepTime);
            _stepIndex++;
        }

        private void RunDrivers()
        {
            var offset = (Seed + _stepIndex) % _drivers.Count;
            for (var slot = 0; slot < _drivers.Count; slot++)
            {
                var driver = _drivers[(offset + slot) % _drivers.Count];
                var car = _cars[driver.CarIndex];
                if (car.HasFinished) continue;
                var level = driver.Decide();
                if (level == 0 || !_airspace.TryClaim(Time)) continue;
                RequestBoost(car, level);
            }
        }

        private void ResolveCrossings(float stepStart)
        {
            _finishing.Clear();
            _finishing.AddRange(_crossings);
            _crossings.Clear();
            foreach (var crossing in _finishing)
                crossing.Car.CloseBoostWindow();
            _standings.ReportCrossings(_finishing, stepStart);
            if (_standings.FinishedCount < _cars.Count) return;
            Conclude();
            HasCompleted = true;
        }

        private void HandleCarFinished(ICarProgress car) => _telemetry.RecordFinish(car);

        private void Enqueue(int level) => _pendingRequests.Add(level);

        private void DrainRequests()
        {
            foreach (var level in _pendingRequests)
                RequestBoost(Player, level);
            _pendingRequests.Clear();
        }

        private void RequestBoost(ICar car, int level)
        {
            var outcome = car.Boost.Request(level);
            _telemetry.RecordRequest(car, level, outcome);
            if (!car.IsPlayer) return;
            if (outcome == BoostRequestOutcome.Accepted) OnBoostAccepted?.Invoke(level);
            else OnBoostRejected?.Invoke(level, outcome);
        }

        private void DetachInput()
        {
            if (_input == null) return;
            _input.OnBoostRequested -= Enqueue;
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

        private AiProfile ProfileFor(int carIndex) => _config.rivals[_slotToProfile[carIndex - 1]].profile;

        private CarSetup BuildSetup(int index, int seed)
        {
            if (index == PlayerIndex)
                return new CarSetup(index, _config.player.displayName, true, _config.player.paint,
                    _laneOffsets[index], _config.race.baseSpeed, _config.track.raceDistance, _config.boost);
            var rival = _config.rivals[_slotToProfile[index - 1]];
            var jitter = DeterministicRandom.Stream(seed, index * JitterSalt);
            var speed = _config.race.baseSpeed * rival.profile.speedScale
                        * (1f + _config.race.speedScaleJitter * jitter.Signed());
            var economy = BoostEconomy.Scaled(_config.boost, rival.profile.energyRegenScale,
                rival.profile.energyCapacityScale,
                rival.profile.energyStartScale * (1f + _config.race.energyStartJitter * jitter.Signed()));
            return new CarSetup(index, rival.displayName, false, rival.paint, _laneOffsets[index],
                speed, _config.track.raceDistance, economy);
        }

        private int[] ShuffledProfiles(int seed)
        {
            var slots = new int[_config.rivals.Length];
            for (var index = 0; index < slots.Length; index++)
                slots[index] = index;
            var random = DeterministicRandom.Stream(seed, LayoutSalt);
            for (var index = slots.Length - 1; index > 0; index--)
            {
                var swap = random.Range(0, index + 1);
                (slots[index], slots[swap]) = (slots[swap], slots[index]);
            }
            return slots;
        }

        private RaceRunInfo BuildRunInfo() => new()
        {
            seed = Seed,
            scenario = string.Empty,
            raceDistance = _config.track.raceDistance,
            baseSpeed = _config.race.baseSpeed,
            logicStep = LogicStep,
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
