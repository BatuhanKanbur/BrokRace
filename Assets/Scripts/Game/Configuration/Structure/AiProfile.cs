using System;
using UnityEngine;

namespace Game.Configuration.Structure
{
    [Serializable]
    public class AiProfile
    {
        public string name = "Rival";
        public float speedScale = 1f;
        public float decisionInterval = 1.4f;
        public float decisionJitter = 0.45f;
        public float decisionPhase;
        public float aggression = 0.5f;
        public float patience = 0.5f;
        public Vector2 attackWindow = new(0f, 1f);
        public float attackWindowWeight = 1f;
        public float chaseWeight = 1f;
        public float defendWeight = 0.5f;
        public float playerFocus = 0.5f;
        public float strikeRange = 45f;
        public float energyFloor = 0.15f;
        public float noise = 0.12f;
        public int levelCap = 5;
    }
}
