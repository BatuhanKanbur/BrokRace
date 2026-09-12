using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.UI.Constants
{
    public static class HudConstants
    {
        public const string ReadyLabel = "READY";
        public const string CountdownHint = "PRESS 1 - 5 TO BOOST";
        public const string OverlayHeader = "#  CAR            DIST   SPEED      GAP  BUF  NRG   BAL";
        public const float BoostWindowReference = 1f;
        public const float FeedbackFadeDuration = 0.7f;
        public const float OverlayFadeDuration = 0.15f;
        public const Key ToggleKey = Key.F1;
        public static readonly Color ReadyColor = new(0.85f, 0.9f, 0.95f, 0.9f);
        public static readonly Color BlockedColor = new(0.35f, 0.35f, 0.4f, 0.55f);
        public static readonly Color RejectColor = new(1f, 0.35f, 0.3f);
    }
}
