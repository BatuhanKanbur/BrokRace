using UnityEngine;

namespace Game.UI.Constants
{
    public static class HudConstants
    {
        public const string ReadyLabel = "READY";
        public const float BoostWindowReference = 1f;
        public const float FeedbackFadeDuration = 0.7f;
        public static readonly Color ReadyColor = new(0.85f, 0.9f, 0.95f, 0.9f);
        public static readonly Color BlockedColor = new(0.35f, 0.35f, 0.4f, 0.55f);
        public static readonly Color RejectColor = new(1f, 0.35f, 0.3f);
    }
}
