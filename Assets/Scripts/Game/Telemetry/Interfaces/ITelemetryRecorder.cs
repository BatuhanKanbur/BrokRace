using System.Collections.Generic;
using Game.Boost.Enums;
using Game.Cars.Interfaces;
using Game.Telemetry.Structure;

namespace Game.Telemetry.Interfaces
{
    public interface ITelemetryRecorder
    {
        public IReadOnlyList<CarSample> Samples { get; }
        public IReadOnlyList<RaceEvent> Events { get; }
        public void Begin(RaceRunInfo info);
        public void Step(float stepTime);
        public void RecordRequest(ICarProgress car, int level, BoostRequestOutcome outcome);
        public void RecordFinish(ICarProgress car);
        public RaceReport Complete();
        public void Reset();
    }
}
