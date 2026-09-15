using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Ai.Interfaces;
using Game.Balancing.Interfaces;
using Game.Boost.Enums;
using Game.Standings.Interfaces;
using Game.Telemetry.Interfaces;
using Game.Telemetry.Structure;

namespace Game.Race.Interfaces
{
    public interface IRaceManager
    {
        public event Action<int> OnBoostAccepted;
        public event Action<int, BoostRequestOutcome> OnBoostRejected;
        public bool HasCompleted { get; }
        public IRaceState State { get; }
        public IRaceOrder Order { get; }
        public IRaceBalancer Balancer { get; }
        public ITelemetryLog Log { get; }
        public IReadOnlyList<IAiDriver> Drivers { get; }
        public UniTask Build();
        public void Prepare(int seed);
        public void SetLaunch(float progress);
        public void Begin();
        public void PumpInput();
        public void Tick(float frameTime);
        public void Present(float deltaTime);
        public void Conclude();
        public RaceReport BuildReport(string scenario, int frameRateTarget);
        public void Teardown();
    }
}
