using Game.Effects.Interfaces;
using UnityEngine;
using static Game.Effects.Constants.EffectConstants;

namespace Game.Effects.Managers
{
    public class CarEffects : ICarEffects
    {
        private readonly IEffectEmitter[] _thrusters;
        private readonly IEffectEmitter _sparks;
        private readonly IEffectEmitter _dust;
        private readonly IEffectEmitter _shockwave;
        private readonly IEffectEmitter _confetti;
        private readonly Light _flare;

        private float _reach;
        private bool _isBoosting;

        public CarEffects(ICarEffectRig rig)
        {
            _thrusters = rig.Thrusters;
            _sparks = rig.Sparks;
            _dust = rig.Dust;
            _shockwave = rig.Shockwave;
            _confetti = rig.Confetti;
            _flare = rig.Flare;
        }

        public void Play(int level, Color color)
        {
            _isBoosting = true;
            var scale = BurstBaseScale + level * BurstScalePerLevel;
            foreach (var thruster in _thrusters)
                thruster.Tint(color);
            _sparks.Tint(color);
            _sparks.Burst(scale);
            _shockwave.Tint(new Color(color.r, color.g, color.b, ShockwaveAlpha));
            _shockwave.Burst(scale);
            _flare.color = color;
        }

        public void Stop() => _isBoosting = false;

        public void Present(float speed, float intensity, float deltaTime)
        {
            _dust.SetRate(Mathf.Clamp01(speed / DustFullSpeed));
            var reach = Mathf.Clamp01(intensity);
            if (_isBoosting)
            {
                _reach = Mathf.MoveTowards(_reach, reach, FlareRiseSpeed * deltaTime);
                foreach (var thruster in _thrusters)
                    thruster.SetRate(ThrusterFloor + reach);
            }
            else
            {
                _reach = Mathf.MoveTowards(_reach, 0f, FlareFadeSpeed * deltaTime);
                foreach (var thruster in _thrusters)
                    thruster.SetRate(0f);
            }
            _flare.intensity = FlareIntensity * _reach;
            _flare.enabled = _reach > FlareEpsilon;
        }

        public void Celebrate(Color color)
        {
            _confetti.Tint(color);
            _confetti.Burst(CelebrationScale);
        }

        public void Reset()
        {
            _isBoosting = false;
            _reach = 0f;
            _flare.enabled = false;
            _flare.intensity = 0f;
            _sparks.Halt();
            _shockwave.Halt();
            _confetti.Halt();
            foreach (var thruster in _thrusters)
            {
                thruster.SetRate(0f);
                thruster.Run();
            }
            _dust.SetRate(0f);
            _dust.Run();
        }
    }
}
