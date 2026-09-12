using System;
using System.Collections.Generic;
using Core.DI.Attributes;
using Cysharp.Threading.Tasks;
using Game.Ai.Interfaces;
using Game.Balancing.Interfaces;
using Game.Boost.Enums;
using Game.Camera.Interfaces;
using Game.Cars.Interfaces;
using Game.Configuration.Interfaces;
using Game.Configuration.Structure;
using Game.Input.Interfaces;
using Game.Race.Interfaces;
using Game.Race.Managers;
using Game.Standings.Interfaces;
using Game.Telemetry.Interfaces;
using Game.Telemetry.Structure;
using Game.Track.Interfaces;
using PoolManager.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static Game.Race.Constants.RaceConstants;
using Pool = PoolManager.Runtime.PoolManager;

namespace Game.Race.Behaviours
{
    public class RaceManager : MonoBehaviour, IRaceManager
    {
        [SerializeField] private Transform carRoot;

        [Inject] private IRaceConfigService _configService;
        [Inject] private ITrackBuilder _trackBuilder;
        [Inject] private IRaceCamera _camera;

        private readonly List<ICarView> _views = new();

        private RaceLoop _loop;
        private RaceConfig _config;
        private float _accumulator;

        public event Action<int> OnBoostAccepted;
        public event Action<int, BoostRequestOutcome> OnBoostRejected;

        public bool HasCompleted => _loop.HasCompleted;
        public IRaceState State => _loop;
        public IRaceOrder Order => _loop.Order;
        public IRaceBalancer Balancer => _loop.Balancer;
        public ITelemetryLog Log => _loop.Log;
        public IReadOnlyList<IAiDriver> Drivers => _loop.Drivers;

        public async UniTask Build()
        {
            _config = _configService.Config;
            _loop = new RaceLoop(_config);
            _loop.OnBoostAccepted += level => OnBoostAccepted?.Invoke(level);
            _loop.OnBoostRejected += (level, outcome) => OnBoostRejected?.Invoke(level, outcome);
            await _trackBuilder.Build(_config.track);
            var cars = await SpawnGrid();
            _loop.Register(cars, BuildLaneOffsets(cars.Count));
            _camera.Follow(_views[PlayerIndex].Transform);
        }

        public void Prepare(int seed, IBoostInputSource input)
        {
            _accumulator = 0f;
            _loop.Prepare(seed, input);
            _camera.Snap();
            Present(0f);
        }

        public void Begin() => _loop.Begin();

        public void PumpInput() => _loop.PumpInput();

        public void Tick(float frameTime)
        {
            _loop.PollInput();
            _accumulator += Mathf.Min(frameTime, _config.race.maxFrameTime);
            var step = _loop.LogicStep;
            while (_accumulator >= step && _loop.IsRunning)
            {
                _loop.Step(step);
                _accumulator -= step;
            }
        }

        public void Present(float deltaTime)
        {
            foreach (var view in _views)
                view.Present(deltaTime);
            _camera.Present(deltaTime);
        }

        public void Conclude() => _loop.Conclude();

        public RaceReport BuildReport(string scenario, int frameRateTarget) =>
            _loop.BuildReport(scenario, frameRateTarget);

        public void Teardown()
        {
            foreach (var view in _views)
                view.Release();
            _views.Clear();
        }

        private async UniTask<List<ICar>> SpawnGrid()
        {
            var cars = new List<ICar> { await SpawnCar(_config.player.prefab) };
            foreach (var rival in _config.rivals)
                cars.Add(await SpawnCar(rival.prefab));
            return cars;
        }

        private async UniTask<ICar> SpawnCar(AssetReference prefab)
        {
            var instance = await Pool.GetObjectAsync(prefab).SetParent(carRoot);
            _views.Add(instance.GetComponent<ICarView>());
            return instance.GetComponent<ICar>();
        }

        private float[] BuildLaneOffsets(int count)
        {
            var lanes = BuildLaneOrder(count);
            var offsets = new float[count];
            for (var index = 0; index < count; index++)
                offsets[index] = (lanes[index] - (count - 1) * 0.5f) * _config.track.laneSpacing;
            return offsets;
        }

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
