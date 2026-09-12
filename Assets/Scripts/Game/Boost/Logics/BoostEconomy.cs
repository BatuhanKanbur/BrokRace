using Game.Configuration.Structure;
using static Game.Boost.Constants.BoostConstants;

namespace Game.Boost.Logics
{
    public static class BoostEconomy
    {
        public static float MetresPerEnergy(BoostSettings settings, float baseSpeed, int level) =>
            settings.levelCosts[level - MinLevel] > 0f
                ? (level - NeutralLevel) * baseSpeed * settings.windowDuration / settings.levelCosts[level - MinLevel]
                : 0f;

        public static float Efficiency(BoostSettings settings, float baseSpeed, int level, int reference) =>
            MetresPerEnergy(settings, baseSpeed, level) / MetresPerEnergy(settings, baseSpeed, reference);

        public static BoostSettings Scaled(BoostSettings source, float regenScale, float capacityScale, float startScale)
        {
            var capacity = source.energyCapacity * capacityScale;
            return new BoostSettings
            {
                windowDuration = source.windowDuration,
                cooldown = source.cooldown,
                energyCapacity = capacity,
                energyOnStart = capacity * startScale,
                energyRegenPerSecond = source.energyRegenPerSecond * regenScale,
                levelCosts = source.levelCosts,
                levelColors = source.levelColors
            };
        }
    }
}
