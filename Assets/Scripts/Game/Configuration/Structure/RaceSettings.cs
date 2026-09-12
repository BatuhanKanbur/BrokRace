using System;
using UnityEngine;

namespace Game.Configuration.Structure
{
    [Serializable]
    public class RaceSettings
    {
        public float baseSpeed = 16f;
        [Range(1f, 6f)] public float countdownSeconds = 3f;
        public float logicStep = 1f / 120f;
        public float maxFrameTime = 0.1f;
        public int seed = 20250912;
        public bool randomizeSeed;
        [Range(0f, 0.06f)] public float speedScaleJitter = 0.02f;
        [Range(0f, 0.2f)] public float energyStartJitter = 0.08f;
    }
}
