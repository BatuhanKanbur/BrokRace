using Game.Boost.Enums;

namespace Game.Utilities
{
    public static class BoostFeedback
    {
        public static string Describe(BoostRequestOutcome outcome) => outcome switch
        {
            BoostRequestOutcome.WindowActive => "BOOST ALREADY RUNNING",
            BoostRequestOutcome.OnCooldown => "COOLING DOWN",
            BoostRequestOutcome.InsufficientEnergy => "NOT ENOUGH ENERGY",
            BoostRequestOutcome.NotRunning => "RACE NOT RUNNING",
            _ => "BOOST"
        };
    }
}
