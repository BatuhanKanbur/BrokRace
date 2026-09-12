using Game.Boost.Enums;
using Game.Configuration.Structure;
using UnityEngine;
using static Game.Boost.Constants.BoostConstants;

namespace Game.Boost.Logics
{
    public static class BoostCurves
    {
        public static float Integral(BoostCurve curve, float progress, BoostSettings settings)
        {
            var t = Mathf.Clamp01(progress);
            return curve switch
            {
                BoostCurve.Smooth => t * t * (3f - 2f * t),
                BoostCurve.Punch => 1f - Mathf.Pow(1f - t, settings.punchSharpness),
                BoostCurve.TwoStep => t + settings.twoStepDepth / Tau * Mathf.Sin(Tau * t),
                BoostCurve.Surge => Mathf.Pow(t, settings.surgeSharpness),
                _ => t
            };
        }

        public static float Rate(BoostCurve curve, float progress, BoostSettings settings)
        {
            var t = Mathf.Clamp01(progress);
            return curve switch
            {
                BoostCurve.Smooth => 6f * t * (1f - t),
                BoostCurve.Punch => settings.punchSharpness * Mathf.Pow(1f - t, settings.punchSharpness - 1f),
                BoostCurve.TwoStep => 1f + settings.twoStepDepth * Mathf.Cos(Tau * t),
                BoostCurve.Surge => settings.surgeSharpness * Mathf.Pow(t, settings.surgeSharpness - 1f),
                _ => 1f
            };
        }

        public static float FrontBias(BoostCurve curve, BoostSettings settings) =>
            Integral(curve, HalfWindow, settings);

        public static string Describe(BoostCurve curve) => curve switch
        {
            BoostCurve.Smooth => "SMOOTH",
            BoostCurve.Punch => "PUNCH",
            BoostCurve.TwoStep => "TWO-STEP",
            BoostCurve.Surge => "SURGE",
            _ => "FLAT"
        };
    }
}
