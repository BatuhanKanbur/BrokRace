using Game.Cars.Interfaces;
using Game.Cars.Structure;
using Game.Utilities;
using UnityEngine;
using static Game.Cars.Constants.CarConstants;

namespace Game.Cars.Managers
{
    public class CarVisual : ICarVisual
    {
        private readonly PaintSlot[] _paintSlots;
        private readonly Transform[] _wheels;
        private readonly Transform _body;
        private readonly TrailRenderer[] _trails;
        private readonly MaterialPropertyBlock _block = new();
        private readonly float _wheelRadius;
        private readonly Vector3 _bodyScale;

        private Color _paint = Color.white;
        private Color _glow;
        private float _glowStrength;
        private float _targetGlow;
        private float _squash;

        public CarVisual(PaintSlot[] paintSlots, Transform[] wheels, Transform body, TrailRenderer[] trails, float wheelRadius)
        {
            _paintSlots = paintSlots;
            _wheels = wheels;
            _body = body;
            _trails = trails;
            _wheelRadius = wheelRadius;
            _bodyScale = body.localScale;
        }

        public void Paint(Color color)
        {
            _paint = color;
            foreach (var trail in _trails)
            {
                trail.startColor = color;
                trail.endColor = new Color(color.r, color.g, color.b, 0f);
            }
            Apply();
        }

        public void PlayBoost(int level)
        {
            _glow = BoostPalette.ForLevel(level);
            _targetGlow = (level - 1) / 4f;
            _squash = BoostSquash * _targetGlow;
            foreach (var trail in _trails)
            {
                trail.startColor = _glow;
                trail.endColor = new Color(_glow.r, _glow.g, _glow.b, 0f);
                trail.widthMultiplier = TrailWidth * _targetGlow;
                trail.time = TrailDuration;
                trail.emitting = _targetGlow > 0f;
            }
        }

        public void StopBoost()
        {
            _targetGlow = 0f;
            foreach (var trail in _trails)
                trail.emitting = false;
        }

        public void Tick(float speed, float deltaTime)
        {
            var spin = speed / (2f * Mathf.PI * _wheelRadius) * FullCircle * deltaTime;
            foreach (var wheel in _wheels)
                wheel.Rotate(Vector3.right, spin, Space.Self);

            var rate = _targetGlow > _glowStrength ? EmissionRiseSpeed : EmissionFadeSpeed;
            _glowStrength = Mathf.MoveTowards(_glowStrength, _targetGlow, rate * deltaTime);
            _squash = Mathf.MoveTowards(_squash, 0f, SquashRecoverSpeed * deltaTime * BoostSquash);
            _body.localScale = new Vector3(
                _bodyScale.x * (1f - _squash),
                _bodyScale.y * (1f - _squash),
                _bodyScale.z * (1f + _squash * 2f));
            Apply();
        }

        public void Reset()
        {
            _targetGlow = 0f;
            _glowStrength = 0f;
            _squash = 0f;
            _body.localScale = _bodyScale;
            foreach (var trail in _trails)
            {
                trail.emitting = false;
                trail.Clear();
            }
            Apply();
        }

        private void Apply()
        {
            var emission = _glow * (_glowStrength * EmissionPeak);
            foreach (var slot in _paintSlots)
            {
                slot.targetRenderer.GetPropertyBlock(_block, slot.materialIndex);
                _block.SetColor(BaseColorProp, _paint);
                _block.SetColor(EmissionColorProp, emission);
                slot.targetRenderer.SetPropertyBlock(_block, slot.materialIndex);
            }
        }
    }
}
