using Game.Telemetry.Structure;

namespace Game.Telemetry.Interfaces
{
    public interface ITelemetryWriter
    {
        public string Write(ITelemetryLog log, RaceReport report, string runName);
    }
}
