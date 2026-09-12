using System.Collections.Generic;
using Game.Cars.Interfaces;
using Game.Configuration.Structure;
using Game.Input.Interfaces;
using Game.Race.Interfaces;
using Game.Race.Managers;
using Game.Telemetry.Interfaces;
using Game.Telemetry.Structure;
using UnityEngine;
using static Game.Race.Constants.RaceConstants;
using static Game.Simulation.Constants.SimulationConstants;

namespace Game.Simulation.Managers
{
    public class RaceSimulation
    {
        private readonly RaceConfig _config;
        private readonly RaceLoop _loop;
        private readonly List<ICar> _cars = new();

        public RaceSimulation(RaceConfig config)
        {
            _config = config;
            _loop = new RaceLoop(config);
            for (var index = 0; index < GridSize; index++)
                _cars.Add(new SimulatedCar());
            _loop.Register(_cars, new float[GridSize]);
        }

        public IRaceState State => _loop;
        public ITelemetryLog Log => _loop.Log;

        public RaceReport Run(int seed, string scenario, IBoostInputSource input, float stepTime)
        {
            _loop.Prepare(seed, input);
            _loop.Begin();
            var steps = 0;
            var limit = (int)(MaxRaceSeconds / stepTime);
            while (!_loop.HasCompleted && steps < limit)
            {
                _loop.Step(stepTime);
                steps++;
            }
            _loop.Conclude();
            return _loop.BuildReport(scenario, (int)(1f / stepTime));
        }

        public RaceReport RunFramed(int seed, string scenario, IBoostInputSource input, float frameTime)
        {
            _loop.Prepare(seed, input);
            _loop.Begin();
            var step = _loop.LogicStep;
            var accumulator = 0f;
            var frames = 0;
            var limit = (int)(MaxRaceSeconds / frameTime);
            while (!_loop.HasCompleted && frames < limit)
            {
                input.Poll();
                accumulator += Mathf.Min(frameTime, _config.race.maxFrameTime);
                while (accumulator >= step && !_loop.HasCompleted)
                {
                    _loop.Step(step);
                    accumulator -= step;
                }
                frames++;
            }
            _loop.Conclude();
            return _loop.BuildReport(scenario, Mathf.RoundToInt(1f / frameTime));
        }
    }
}
