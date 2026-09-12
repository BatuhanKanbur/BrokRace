using System;

namespace Game.Telemetry.Structure
{
    [Serializable]
    public struct RaceEvent
    {
        public float time;
        public int carIndex;
        public string kind;
        public int level;
        public string detail;
    }
}
