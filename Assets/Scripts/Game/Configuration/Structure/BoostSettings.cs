using System;
using UnityEngine;

namespace Game.Configuration.Structure
{
    [Serializable]
    public class BoostSettings
    {
        public float windowDuration = 1f;
        [Range(0f, 3f)] public float cooldown = 0.35f;
        public float energyCapacity = 100f;
        public float energyOnStart = 75f;
        public float energyRegenPerSecond = 5f;
        public float[] levelCosts = { 0f, 14f, 29f, 44f, 60f };
        public Color[] levelColors =
        {
            new(0.75f, 0.78f, 0.82f),
            new(0.35f, 0.80f, 1f),
            new(0.45f, 1f, 0.50f),
            new(1f, 0.75f, 0.20f),
            new(1f, 0.35f, 0.25f)
        };
    }
}
