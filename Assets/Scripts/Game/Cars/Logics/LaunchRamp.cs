using UnityEngine;
using static Game.Cars.Constants.CarConstants;

namespace Game.Cars.Logics
{
    public static class LaunchRamp
    {
        public static float Rate(float progress)
        {
            var t = Mathf.Clamp01(progress);
            return t * t * (3f - 2f * t);
        }

        public static float Offset(float progress)
        {
            var t = Mathf.Clamp01(progress);
            return t * t * t * (1f - t * LaunchArea) - LaunchArea;
        }
    }
}
