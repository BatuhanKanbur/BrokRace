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
                BoostCurve.Smooth => SmoothIntegral(t),
                BoostCurve.Punch => Mathf.Lerp(SmoothIntegral(t), FrontIntegral(t), settings.curveSkew),
                BoostCurve.TwoStep => TwoStepIntegral(t, settings.twoStepDepth),
                BoostCurve.Surge => Mathf.Lerp(SmoothIntegral(t), BackIntegral(t), settings.curveSkew),
                BoostCurve.Plateau => PlateauIntegral(t, settings.plateauRamp),
                _ => t
            };
        }

        public static float Rate(BoostCurve curve, float progress, BoostSettings settings)
        {
            var t = Mathf.Clamp01(progress);
            return curve switch
            {
                BoostCurve.Smooth => SmoothRate(t),
                BoostCurve.Punch => Mathf.Lerp(SmoothRate(t), FrontRate(t), settings.curveSkew),
                BoostCurve.TwoStep => TwoStepRate(t, settings.twoStepDepth),
                BoostCurve.Surge => Mathf.Lerp(SmoothRate(t), BackRate(t), settings.curveSkew),
                BoostCurve.Plateau => PlateauRate(t, settings.plateauRamp),
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
            BoostCurve.Plateau => "PLATEAU",
            _ => "FLAT"
        };

        private static float SmoothRate(float t) => 6f * t * (1f - t);

        private static float SmoothIntegral(float t) => t * t * (3f - 2f * t);

        private static float FrontRate(float t) => 12f * t * (1f - t) * (1f - t);

        private static float FrontIntegral(float t) => t * t * (6f + t * (3f * t - 8f));

        private static float BackRate(float t) => 12f * t * t * (1f - t);

        private static float BackIntegral(float t) => t * t * t * (4f - 3f * t);

        private static float TwoStepRate(float t, float depth) =>
            (1f - Mathf.Cos(Tau * t)) * (1f - depth * Mathf.Cos(2f * Tau * t));

        private static float TwoStepIntegral(float t, float depth) =>
            t
            - depth * Mathf.Sin(2f * Tau * t) / (2f * Tau)
            - (1f - depth * HalfWindow) * Mathf.Sin(Tau * t) / Tau
            + depth * HalfWindow * Mathf.Sin(3f * Tau * t) / (3f * Tau);

        private static float PlateauRate(float t, float ramp)
        {
            var height = 1f / (1f - ramp);
            if (t < ramp) return height * SmoothIntegral(t / ramp);
            if (t > 1f - ramp) return height * SmoothIntegral((1f - t) / ramp);
            return height;
        }

        private static float PlateauIntegral(float t, float ramp)
        {
            var height = 1f / (1f - ramp);
            if (t < ramp) return height * ramp * RampArea(t / ramp);
            if (t > 1f - ramp) return 1f - height * ramp * RampArea((1f - t) / ramp);
            return height * (ramp * HalfWindow + t - ramp);
        }

        private static float RampArea(float p) => p * p * p * (1f - p * HalfWindow);
    }
}
