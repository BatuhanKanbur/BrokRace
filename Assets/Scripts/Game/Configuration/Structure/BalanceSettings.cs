using System;

namespace Game.Configuration.Structure
{
    [Serializable]
    public class BalanceSettings
    {
        public bool enabled = true;
        public float deadZone = 40f;
        public float fullEffectGap = 200f;
        public float maxCatchUpScale = 1.06f;
        public float minLeaderScale = 0.94f;
        public float leaderRunawayGap = 160f;
        public float changeRatePerSecond = 0.15f;
        public float fadeStartProgress = 0.85f;
    }
}
