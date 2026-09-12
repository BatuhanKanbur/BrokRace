using Game.Configuration.Structure;
using UnityEngine;
using static Game.Boost.Constants.BoostConstants;

namespace Game.Boost.Logics
{
    public static class BoostPalette
    {
        public static Color ForLevel(BoostSettings settings, int level) =>
            settings.levelColors[Mathf.Clamp(level, MinLevel, MaxLevel) - MinLevel];
    }
}
