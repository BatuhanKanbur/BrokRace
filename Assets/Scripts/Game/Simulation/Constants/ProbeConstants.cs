namespace Game.Simulation.Constants
{
    public static class ProbeConstants
    {
        public const float NoFinish = float.MaxValue;
        public const int ProbeLevel = 3;
        public const int OverflowLevel = 3;
        public const float DistanceTolerance = 1e-4f;
        public const float DeviationBudget = 1e-3f;
        public const float CurveSeparation = 0.05f;
        public const float CrossingSeconds = 4f;
        public const float CrossingOverrun = 1.5f;
        public const float CrossingTolerance = 1e-3f;
        public const float NoCrossingTime = -1f;
        public const float SameStepSpeedRatio = 1.35f;
        public const float EarlyCrossFraction = 0.18f;
        public const float LateCrossFraction = 0.81f;
    }
}
