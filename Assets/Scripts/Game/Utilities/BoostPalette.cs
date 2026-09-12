using UnityEngine;

namespace Game.Utilities
{
    public static class BoostPalette
    {
        public static Color ForLevel(int level) => level switch
        {
            2 => new Color(0.35f, 0.8f, 1f),
            3 => new Color(0.45f, 1f, 0.5f),
            4 => new Color(1f, 0.75f, 0.2f),
            5 => new Color(1f, 0.35f, 0.25f),
            _ => new Color(0.75f, 0.75f, 0.8f)
        };
    }
}
