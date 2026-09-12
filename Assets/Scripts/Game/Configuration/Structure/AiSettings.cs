using System;

namespace Game.Configuration.Structure
{
    [Serializable]
    public class AiSettings
    {
        public float costWeight = 4f;
        public float actionThreshold = 0.35f;
        public float reserveUrgency = 3f;
    }
}
