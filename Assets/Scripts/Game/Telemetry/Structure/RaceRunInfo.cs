using System;

namespace Game.Telemetry.Structure
{
    [Serializable]
    public class RaceRunInfo
    {
        public int seed;
        public string scenario;
        public float raceDistance;
        public float baseSpeed;
        public float logicStep;
        public float boostWindow;
        public int frameRateTarget;
        public bool balancingEnabled;
    }
}
