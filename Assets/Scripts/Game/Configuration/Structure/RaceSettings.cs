using System;

namespace Game.Configuration.Structure
{
    [Serializable]
    public class RaceSettings
    {
        public float baseSpeed = 16f;
        public float countdownSeconds = 3f;
        public float logicStep = 1f / 120f;
        public float maxFrameTime = 0.1f;
        public int seed = 20250912;
        public bool randomizeSeed;
    }
}
