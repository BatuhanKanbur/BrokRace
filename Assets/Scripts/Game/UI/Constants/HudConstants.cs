using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.UI.Constants
{
    public static class HudConstants
    {
        public const string ReadyLabel = "READY";
        public const string GoLabel = "GO!";
        public const string PlayerLabel = "player";
        public const string ActiveLabel = "balancer active";
        public const string SuspendedLabel = "balancer suspended";
        public const string CountdownHint = "PRESS 1 - 5 TO BOOST";
        public const string WinnerLabel = "WINNER";
        public const string FinishedFormat = "FINISHED {0}/{1}";
        public const string ResultRowFormat = "{0}.  {1,-12} {2,6:0.00}s   {3,6:+0.00;-0.00;0.00}";
        public const string OverlayHeaderFormat = "seed {0}   t {1:0.00}s   step {2:0.00}ms   samples {3}   {4}";
        public const string OverlayColumns = "#  CAR            DIST  SPEED     GAP  BUF  NRG    BAL  ASSIST  STATE";
        public const string OverlayRowFormat = "{0}  {1,-10} {2,7:0.0} {3,6:0.0} {4,7:+0.0;-0.0;0.0}   x{5} {6,4:0} {7,6:0.000} {8,7:+0.0;-0.0;0.0}  {9}";
        public const string OverlayStateFormat = "{0} lvl{1} s{2:0.00} d{3:0.00} p{4:0.00} c{5:0.00} e{6:0.00} score{7:0.00} hold{8:0.00}";
        public const string SpeedFormat = "{0} m/s";
        public const string RemainingFormat = "{0} m";
        public const string RankFormat = "{0}/{1}";
        public const string ActiveBoostFormat = "x{0}  {1:0.00}s";
        public const string AcceptedFormat = "BOOST x{0}";
        public const float MillisPerSecond = 1000f;
        public const float FeedbackFadeDuration = 0.7f;
        public const float OverlayFadeDuration = 0.15f;
        public const Key ToggleOverlayKey = Key.F1;
        public static readonly Color ReadyColor = new(0.85f, 0.9f, 0.95f, 0.9f);
        public static readonly Color BlockedColor = new(0.35f, 0.35f, 0.4f, 0.55f);
        public static readonly Color RejectColor = new(1f, 0.35f, 0.3f);
    }
}
