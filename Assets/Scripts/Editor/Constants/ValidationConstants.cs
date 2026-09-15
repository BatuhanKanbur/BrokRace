namespace Editor.Constants
{
    public static class ValidationConstants
    {
        public const string ConfigPath = "Assets/Data/RaceConfig.asset";
        public const string ValidationFolder = "Docs/Validation";
        public const string ContractFile = "buff_contract.csv";
        public const string RuleFile = "buff_rules.csv";
        public const string MatrixFile = "race_matrix.csv";
        public const string RivalFile = "race_cars.csv";
        public const string FrameFile = "frame_rate.csv";
        public const string LifecycleFile = "lifecycle.csv";
        public const string NumberFormat = "0.###";
        public const string PercentFormat = "0.######";
        public const string PassLabel = "PASS";
        public const string FailLabel = "FAIL";
        public const string ZeroLabel = "0";
        public const float PercentScale = 100f;
        public const string MenuPath = "Tools/BrokRace/Run Race Validation";
        public const string ProgressTitle = "Race Validation";
        public const string ContractStep = "Boost contract";
        public const string MatrixStep = "Race matrix";
        public const string FrameStep = "Frame rate";
        public const string LifecycleStep = "Lifecycle";
        public const float ContractProgress = 0f;
        public const float MatrixProgress = 0.2f;
        public const float FrameProgress = 0.75f;
        public const float LifecycleProgress = 0.95f;
        public const string ContractHeader = "level,curve,logicStepsPerSecond,baseSpeed,expectedWindowDistance,measuredWindowDistance,expectedExtra,measuredExtra,boostedSeconds,deviationPercent";
        public const string RuleHeader = "rule,result,detail";
        public const string MatrixHeader = "scenario,seed,playerFinishTime,playerRank,accepted,rejected,energySpent,energyWasted,boostDistance,expectedBoostDistance,assistDistance,overtakesMade,overtakesConceded,finalGapAhead,finalGapBehind,finalGapToLeader,avgGapAheadLast20,avgGapBehindLast20,avgGapToLeaderLast20,packSpreadAtWinnerFinish,avgPackSpread,maxPackSpread,leadChanges";
        public const string RivalHeader = "scenario,seed,car,profile,isPlayer,finishOrder,finishTime,gapToPlayerSeconds,accepted,rejected,energySpent,energyWasted,boostDistance,assistDistance,averageSpeed";
        public const string FrameHeader = "seed,frameRate,playerFinishTime,playerRank,boostDistance,expectedBoostDistance,boostDeviationPercent,timeDeviationPercent,gapToLeader";
        public static readonly float[] ProbeSteps = { 1f / 30f, 1f / 60f, 1f / 120f, 1f / 37f };
        public static readonly int[] Seeds = { 1337, 90210, 777013 };
        public static readonly int[] FrameRates = { 120, 60, 30 };
    }
}
