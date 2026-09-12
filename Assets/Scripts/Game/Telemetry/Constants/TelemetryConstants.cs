namespace Game.Telemetry.Constants
{
    public static class TelemetryConstants
    {
        public const float OvertakeHysteresis = 2f;
        public const float FinalStretchFraction = 0.8f;
        public const string SampleHeader = "time,carIndex,name,isPlayer,rank,distance,speed,boostLevel,energy,balanceScale,gapToPlayer,gapToLeader,aiState";
        public const string EventHeader = "time,carIndex,name,kind,level,detail";
        public const string SampleFileSuffix = "_samples.csv";
        public const string EventFileSuffix = "_events.csv";
        public const string ReportFileSuffix = "_report.json";
        public const string BoostAccepted = "boost_accepted";
        public const string BoostRejected = "boost_rejected";
        public const string Overtake = "overtake";
        public const string Conceded = "conceded";
        public const string Finish = "finish";
        public const string PlayerState = "player";
    }
}
