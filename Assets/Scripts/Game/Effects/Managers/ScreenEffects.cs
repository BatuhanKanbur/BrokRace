using Game.Effects.Interfaces;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static Game.Effects.Constants.ScreenConstants;

namespace Game.Effects.Managers
{
    public class ScreenEffects : IScreenEffects
    {
        private readonly Bloom _bloom;
        private readonly Vignette _vignette;
        private readonly ChromaticAberration _chromatic;
        private readonly ColorAdjustments _grade;
        private readonly MotionBlur _motionBlur;

        private float _charge;
        private float _reach;

        public ScreenEffects(Volume volume)
        {
            var profile = volume.profile;
            profile.TryGet(out _bloom);
            profile.TryGet(out _vignette);
            profile.TryGet(out _chromatic);
            profile.TryGet(out _grade);
            profile.TryGet(out _motionBlur);
            Reset();
        }

        public void Punch(float strength) => _charge = Mathf.Max(_charge, strength);

        public void Present(float intensity, float deltaTime)
        {
            _charge = Mathf.MoveTowards(_charge, 0f, ChargeDecay * deltaTime);
            var target = Mathf.Clamp01(Mathf.Max(intensity, _charge));
            var rate = target > _reach ? ReachRiseSpeed : ReachFadeSpeed;
            _reach = Mathf.MoveTowards(_reach, target, rate * deltaTime);
            Apply();
        }

        public void Reset()
        {
            _charge = 0f;
            _reach = 0f;
            Apply();
        }

        private void Apply()
        {
            _bloom.intensity.value = Mathf.Lerp(BloomBase, BloomPeak, _reach);
            _vignette.intensity.value = Mathf.Lerp(VignetteBase, VignettePeak, _reach);
            _chromatic.intensity.value = Mathf.Lerp(ChromaticBase, ChromaticPeak, _reach);
            _grade.saturation.value = Mathf.Lerp(SaturationBase, SaturationPeak, _reach);
            _motionBlur.intensity.value = Mathf.Lerp(MotionBlurBase, MotionBlurPeak, _reach);
        }
    }
}
