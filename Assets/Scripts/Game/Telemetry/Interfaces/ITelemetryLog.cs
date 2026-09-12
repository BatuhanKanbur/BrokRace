using System.Collections.Generic;
using Game.Telemetry.Structure;

namespace Game.Telemetry.Interfaces
{
    public interface ITelemetryLog
    {
        public IReadOnlyList<CarSample> Samples { get; }
        public IReadOnlyList<RaceEvent> Events { get; }
        public IReadOnlyList<string> CarNames { get; }
    }
}
