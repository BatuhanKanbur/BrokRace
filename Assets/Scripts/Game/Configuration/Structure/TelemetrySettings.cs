using System;

namespace Game.Configuration.Structure
{
    [Serializable]
    public class TelemetrySettings
    {
        public float sampleInterval = 0.1f;
        public string outputDirectory = "Telemetry";
        public bool writeCsv = true;
        public bool writeJson = true;
    }
}
