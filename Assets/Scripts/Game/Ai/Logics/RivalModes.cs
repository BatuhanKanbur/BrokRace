using Game.Ai.Enums;
using static Game.Ai.Constants.AiConstants;

namespace Game.Ai.Logics
{
    public static class RivalModes
    {
        public static string Describe(RivalMode mode) => mode switch
        {
            RivalMode.NoBoost => NoBoostLabel,
            RivalMode.MaxSpam => MaxSpamLabel,
            RivalMode.EarlyBurst => EarlyBurstLabel,
            _ => NormalLabel
        };
    }
}
