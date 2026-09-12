using System;
using UnityEngine;

namespace Game.Configuration.Structure
{
    [Serializable]
    public class AiSettings
    {
        [Range(2, 5)] public int minLevel = 3;
        public float strikeTolerance = 18f;
        public float defendRange = 40f;
        public float paceDeficitSpan = 60f;
        [Range(0.5f, 1f)] public float spillThreshold = 0.85f;
        public float levelFitWeight = 0.35f;
        public float costWeight = 0.35f;
        public float holdSaveWeight = 0.55f;
        [Range(0.8f, 1f)] public float burnDownProgress = 0.9f;
        public float airspaceGate = 0.25f;
        public float noNeighbourGap = 9999f;
    }
}
