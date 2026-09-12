namespace Game.Race.Constants
{
    public static class RaceConstants
    {
        public const int PlayerIndex = 0;
        public const int RivalCount = 7;
        public const int GridSize = RivalCount + 1;
        public const int NoiseSalt = 1000;
        public const int LayoutSalt = 97;
        public const int JitterSalt = 193;
        public const float MinimumLogicStep = 1f / 480f;
        public const string LiveRunPrefix = "live";
    }
}
