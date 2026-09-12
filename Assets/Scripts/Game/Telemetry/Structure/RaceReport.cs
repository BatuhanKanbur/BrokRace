using System;

namespace Game.Telemetry.Structure
{
    [Serializable]
    public class RaceReport
    {
        public RaceRunInfo run;
        public CarSummary[] cars;
        public int playerOvertakesMade;
        public int playerOvertakesConceded;
        public float playerFinishTime;
        public int playerFinishRank;
        public float finalGapToLeader;
        public float finalGapAhead;
        public float finalGapBehind;
        public float averageGapAheadLastFifth;
        public float averageGapBehindLastFifth;
        public float averageGapToLeaderLastFifth;
        public float packSpreadAtFinish;
        public float averagePackSpread;
        public float maxPackSpread;
        public int leadChanges;
        public float playerBoostDistance;
        public float playerExpectedBoostDistance;
        public float playerAssistDistance;
    }
}
