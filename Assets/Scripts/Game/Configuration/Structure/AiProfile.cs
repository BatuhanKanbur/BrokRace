using System;
using UnityEngine;

namespace Game.Configuration.Structure
{
    [Serializable]
    public class AiProfile
    {
        public string name = "Rival";
        [Range(0.9f, 1.1f)] public float speedScale = 1f;
        public float targetFinishTime = 58f;
        public float decisionInterval = 2.9f;
        [Range(0f, 0.5f)] public float decisionJitter = 0.18f;
        [Range(0f, 1f)] public float decisionPhase;
        [Range(0.8f, 1.2f)] public float energyRegenScale = 1f;
        [Range(0.8f, 1.4f)] public float energyCapacityScale = 1f;
        [Range(0.4f, 1f)] public float energyStartScale = 0.8f;
        public float reserveEnergy = 20f;
        [Range(2, 5)] public int levelBias = 4;
        [Range(0.5f, 0.95f)] public float closeFrom = 0.75f;
        [Range(0f, 1.4f)] public float strikeWeight = 0.6f;
        [Range(0f, 1.4f)] public float defendWeight = 0.3f;
        [Range(0f, 1.4f)] public float paceWeight = 0.5f;
        [Range(0f, 1.4f)] public float closeWeight = 0.35f;
        [Range(0f, 1.4f)] public float spillWeight = 0.55f;
        [Range(0f, 1.4f)] public float efficiencyWeight = 0.3f;
        [Range(0f, 1.4f)] public float noiseWeight = 0.2f;
        [Range(0f, 1.4f)] public float holdBias = 0.8f;
        [Range(0f, 1f)] public float balanceResponse = 1f;
    }
}
