using UnityEngine;

namespace Game.Cars.Constants
{
    public static class CarConstants
    {
        public static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");
        public static readonly int EmissionColorProp = Shader.PropertyToID("_EmissionColor");
        public const float FullCircle = 360f;
        public const float NoCrossing = -1f;
        public const float LaneBlendSpeed = 2.2f;
        public const float TrailWidth = 0.7f;
        public const float TrailDuration = 0.35f;
        public const float EmissionRiseSpeed = 12f;
        public const float EmissionFadeSpeed = 3.5f;
        public const float EmissionPeak = 5f;
        public const float BoostSquash = 0.08f;
        public const float SquashRecoverSpeed = 5f;
        public const float RotationBlendSpeed = 9f;
    }
}
