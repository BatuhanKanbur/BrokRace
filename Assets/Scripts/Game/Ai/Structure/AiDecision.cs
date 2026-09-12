namespace Game.Ai.Structure
{
    public readonly struct AiDecision
    {
        public int Level { get; }
        public float Strike { get; }
        public float Defend { get; }
        public float Pace { get; }
        public float Closing { get; }
        public float Spill { get; }
        public float Score { get; }
        public float HoldScore { get; }

        public AiDecision(int level, float strike, float defend, float pace, float closing, float spill,
            float score, float holdScore)
        {
            Level = level;
            Strike = strike;
            Defend = defend;
            Pace = pace;
            Closing = closing;
            Spill = spill;
            Score = score;
            HoldScore = holdScore;
        }
    }
}
