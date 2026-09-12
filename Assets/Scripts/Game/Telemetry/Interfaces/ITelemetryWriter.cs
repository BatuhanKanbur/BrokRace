namespace Game.Telemetry.Interfaces
{
    public interface ITelemetryWriter
    {
        public string Write(ITelemetryRecorder recorder, string runName, string[] carNames);
    }
}
