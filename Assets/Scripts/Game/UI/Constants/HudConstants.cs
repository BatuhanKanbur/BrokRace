using UnityEngine;

namespace Game.UI.Constants
{
    public static class HudConstants
    {
        public const string ReadyLabel = "READY";
        public const string OverlayLabel = "STATS";
        public const string RestartLabel = "RESTART";
        public const string BoostLabelFormat = "x{0}";
        public const string ModesLabel = "MODES";
        public const string SetupTitle = "RACE SETUP";
        public const string RivalCaption = "RIVAL BOOSTS";
        public const string BalanceCaption = "RUBBER BAND";
        public const string StartLabel = "START";
        public const string BalanceOnLabel = "ON";
        public const string BalanceOffLabel = "OFF";
        public const string GoLabel = "GO!";
        public const string PlayerLabel = "player";
        public const string ActiveLabel = "balancer active";
        public const string SuspendedLabel = "balancer suspended";
        public const string CountdownHint = "TAP x1 - x5 TO BOOST";
        public const string WinnerLabel = "WINNER";
        public const string FinishedFormat = "FINISHED {0}/{1}";
        public const string ResultRowFormat = "{0}.  {1,-12} {2,6:0.00}s   {3,6:+0.00;-0.00;0.00}";
        public const string OverlayHeaderFormat = "seed {0}   t {1:0.00}s   step {2:0.00}ms   samples {3}   {4}";
        public const string OverlayColumns = "#  CAR            DIST  SPEED     GAP  BUF  NRG    BAL  ASSIST  STATE";
        public const string OverlayRowFormat = "{0}  {1,-10} {2,7:0.0} {3,6:0.0} {4,7:+0.0;-0.0;0.0}   x{5} {6,4:0} {7,6:0.000} {8,7:+0.0;-0.0;0.0}  {9}";
        public const string OverlayStateFormat = "{0} lvl{1} s{2:0.00} d{3:0.00} p{4:0.00} c{5:0.00} e{6:0.00} score{7:0.00} hold{8:0.00}";
        public const string SpeedFormat = "{0} km/h";
        public const string RemainingFormat = "{0} m";
        public const string RankFormat = "{0}/{1}";
        public const string ActiveBoostFormat = "x{0}  {1}  {2:0.00}s";
        public const string AcceptedFormat = "BOOST x{0}";
        public const float MillisPerSecond = 1000f;
        public const float KilometresPerHour = 3.6f;
        public const float DialLerp = 9f;
        public const float FeedbackFadeDuration = 0.7f;
        public const float FeedbackPunchScale = 1.22f;
        public const float FeedbackPunchDuration = 0.3f;
        public const float RankPulseDuration = 0.45f;
        public const float RankPulseScale = 1.18f;
        public const float RankTintDuration = 0.6f;
        public const float KeyPunchScale = 1.38f;
        public const float KeyPunchDuration = 0.28f;
        public const float LowEnergyRatio = 0.3f;
        public const float LowEnergyPulseRate = 7f;
        public const float EnergyFlashFade = 3.2f;
        public const float CountPunchScale = 1.45f;
        public const float CountPunchDuration = 0.35f;
        public const float GoPunchScale = 1.85f;
        public const float HeadlinePunchScale = 1.3f;
        public const float HeadlinePunchDuration = 0.5f;
        public const float OverlayFadeDuration = 0.15f;
        public static readonly Color ReadyColor = new(0.85f, 0.9f, 0.95f, 0.9f);
        public static readonly Color SelectedColor = new(0.35f, 0.85f, 1f, 0.95f);
        public static readonly string[] MonospaceFonts = { "Consolas", "Courier New", "DejaVu Sans Mono", "Monospace" };
        public static readonly Color BlockedColor = new(0.35f, 0.35f, 0.4f, 0.55f);
        public static readonly Color RejectColor = new(1f, 0.35f, 0.3f);
        public static readonly Color SpeedCalmColor = new(0.78f, 0.86f, 0.95f);
        public static readonly Color SpeedHotColor = new(1f, 0.6f, 0.22f);
        public static readonly Color RankRestColor = new(0.92f, 0.95f, 1f);
        public static readonly Color RankGainColor = new(0.45f, 1f, 0.55f);
        public static readonly Color RankLossColor = new(1f, 0.45f, 0.4f);
        public static readonly Color LowEnergyColor = new(1f, 0.35f, 0.3f);
        public static readonly Color EnergyFlashColor = Color.white;
        public static readonly Color CountdownCountColor = new(0.9f, 0.94f, 1f);
        public static readonly Color CountdownGoColor = new(0.45f, 1f, 0.5f);
        public static readonly Color WinnerColor = new(1f, 0.85f, 0.35f);
        public static readonly Color PlacedColor = new(0.85f, 0.9f, 0.95f);
    }
}
