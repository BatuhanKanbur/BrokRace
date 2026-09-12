using System;
using Cysharp.Threading.Tasks;
using Game.Boost.Enums;
using Game.Cars.Interfaces;
using Game.Input.Interfaces;
using Game.Standings.Interfaces;
using Game.Telemetry.Interfaces;
using Game.Telemetry.Structure;

namespace Game.Race.Interfaces
{
    public interface IRaceManager
    {
        public event Action<ICarProgress> OnCarFinished;
        public event Action OnRaceCompleted;
        public event Action<int> OnBoostAccepted;
        public event Action<int, BoostRequestOutcome> OnBoostRejected;
        public IRaceOrder Order { get; }
        public ITelemetryRecorder Telemetry { get; }
        public UniTask Build();
        public void Prepare(int seed, IBoostInputSource input);
        public void Begin();
        public void Tick(float frameTime);
        public void Present(float deltaTime);
        public void Conclude();
        public RaceReport BuildReport(string scenario, int frameRateTarget);
        public void Teardown();
    }
}
