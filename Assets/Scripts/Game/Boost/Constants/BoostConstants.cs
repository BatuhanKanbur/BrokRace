namespace Game.Boost.Constants
{
    public static class BoostConstants
    {
        public const int MinLevel = 1;
        public const int MaxLevel = 5;
        public const int NeutralLevel = 1;
        public const int LevelCount = MaxLevel - MinLevel + 1;
        public const int LevelSpan = MaxLevel - MinLevel;
        public const float WindowEpsilon = 1e-5f;
        public const float EnergyTolerance = 1e-2f;
        public const float Tau = 6.2831853f;
        public const float NeutralBias = 0.5f;
        public const float HalfWindow = 0.5f;
        public const int CrossingRefinements = 3;
    }
}
