using System;
using UnityEngine;

namespace Game.Configuration.Structure
{
    [Serializable]
    public class BalanceSettings
    {
        public bool enabled = true;
        public float muteBand = 120f;
        public float fullAssistGap = 300f;
        public float runawayGap = 180f;
        public float fullRestraintGap = 400f;
        [Range(1f, 1.1f)] public float maxAssistScale = 1.04f;
        [Range(0.9f, 1f)] public float minRestraintScale = 0.97f;
        [Range(0.01f, 0.3f)] public float changeRatePerSecond = 0.06f;
        [Range(0.5f, 1f)] public float fadeStartProgress = 0.8f;
        [Range(0.5f, 1f)] public float fadeEndProgress = 0.95f;
        public float metreBudget = 45f;
        [Range(0f, 1f)] public float passiveSpendThreshold = 0.3f;
    }
}
