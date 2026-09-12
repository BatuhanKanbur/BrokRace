using System;

namespace Game.Configuration.Structure
{
    [Serializable]
    public class BoostSettings
    {
        public float windowDuration = 1f;
        public float cooldown = 0.35f;
        public float energyCapacity = 100f;
        public float energyOnStart = 100f;
        public float energyRegenPerSecond = 5f;
        public float[] levelCosts = { 0f, 14f, 29f, 46f, 66f };
    }
}
