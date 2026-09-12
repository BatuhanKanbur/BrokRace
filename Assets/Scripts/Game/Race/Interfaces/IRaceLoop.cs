using System;
using System.Collections.Generic;
using Game.Ai.Interfaces;
using Game.Balancing.Interfaces;
using Game.Boost.Enums;
using Game.Cars.Interfaces;
using Game.Input.Interfaces;
using Game.Standings.Interfaces;
using Game.Telemetry.Interfaces;
using Game.Telemetry.Structure;

namespace Game.Race.Interfaces
{
    public interface IRaceLoop
    {
        public event Action<int> OnBoostAccepted;
        public event Action<int, BoostRequestOutcome> OnBoostRejected;
        public bool HasCompleted { get; }
        public IRaceOrder Order { get; }
        public IRaceBalancer Balancer { get; }
        public ITelemetryLog Log { get; }
        public IReadOnlyList<IAiDriver> Drivers { get; }
        public void Register(IReadOnlyList<ICar> cars, float[] laneOffsets);
        public void Prepare(int seed, IBoostInputSource input);
        public void Begin();
        public void PollInput();
        public void PumpInput();
        public void Step(float stepTime);
        public void Conclude();
        public RaceReport BuildReport(string scenario, int frameRateTarget);
    }
}
