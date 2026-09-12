using UnityEngine;

namespace Game.Cars.Constants
{
    public static class CarConstants
    {
        public static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");
        public static readonly int EmissionColorProp = Shader.PropertyToID("_EmissionColor");
        public static readonly int BaseMapProp = Shader.PropertyToID("_BaseMap");
        public const float FullCircle = 360f;
        public const float Tau = 6.2831853f;
        public const float NoCrossing = -1f;
        public const float TrailWidth = 0.32f;
        public const float TrailDuration = 0.16f;
        public const float TrailFloor = 0.18f;
        public const float EmissionRiseSpeed = 12f;
        public const float EmissionFadeSpeed = 3.5f;
        public const float EmissionPeak = 5f;
        public const float BoostSquash = 0.08f;
        public const float SquashStretchRatio = 2f;
        public const float SquashRecoverSpeed = 0.4f;
        public const float RotationBlendSpeed = 9f;
    }
}
