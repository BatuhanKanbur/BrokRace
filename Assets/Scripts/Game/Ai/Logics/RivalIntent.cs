using Game.Ai.Enums;
using static Game.Ai.Constants.AiConstants;
using static Game.Boost.Constants.BoostConstants;

namespace Game.Ai.Logics
{
    public static class RivalIntent
    {
        public static int For(RivalMode mode, int decision, float raceTime, float burstDeadline) => mode switch
        {
            RivalMode.NoBoost => NoDecision,
            RivalMode.MaxSpam => MaxLevel,
            RivalMode.EarlyBurst => raceTime < burstDeadline ? MaxLevel : NoDecision,
            _ => decision
        };
    }
}
