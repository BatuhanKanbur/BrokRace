using Game.Cars.Interfaces;
using Game.Cars.Structure;
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
        private float _squash;
        private bool _isBoosting;
        private bool _cueDirty;

        public CarVisual(PaintSlot[] paintSlots, Transform[] wheels, Transform body, TrailRenderer[] trails,
            float wheelRadius)
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
            Apply();
        }

        public void PlayBoost(int level, Color color)
        {
            _glow = color;
            _isBoosting = true;
            _cueDirty = true;
        }

        public void StopBoost()
        {
            _isBoosting = false;
            _cueDirty = true;
        }

        public void Present(float speed, float intensity, float deltaTime)
        {
            var spin = speed / (Tau * _wheelRadius) * FullCircle * deltaTime;
            foreach (var wheel in _wheels)
                wheel.Rotate(Vector3.right, spin, Space.Self);

            if (_cueDirty) FlushCue();
            var reach = Mathf.Clamp01(intensity);
            if (_isBoosting)
            {
                foreach (var trail in _trails)
                    trail.widthMultiplier = TrailWidth * (TrailFloor + reach);
                _glowStrength = Mathf.MoveTowards(_glowStrength, reach, EmissionRiseSpeed * deltaTime);
                _squash = Mathf.Max(_squash, BoostSquash * reach);
            }
            else
            {
                _glowStrength = Mathf.MoveTowards(_glowStrength, 0f, EmissionFadeSpeed * deltaTime);
            }
            _squash = Mathf.MoveTowards(_squash, 0f, SquashRecoverSpeed * deltaTime);
            _body.localScale = new Vector3(
                _bodyScale.x * (1f - _squash),
                _bodyScale.y * (1f - _squash),
                _bodyScale.z * (1f + _squash * SquashStretchRatio));
            Apply();
        }

        public void Reset()
        {
            _isBoosting = false;
            _glowStrength = 0f;
            _squash = 0f;
            _cueDirty = false;
            _body.localScale = _bodyScale;
            foreach (var trail in _trails)
            {
                trail.emitting = false;
                trail.widthMultiplier = 0f;
                trail.Clear();
            }
            Apply();
        }

        private void FlushCue()
        {
            _cueDirty = false;
            var tint = _isBoosting ? _glow : _paint;
            foreach (var trail in _trails)
            {
                trail.startColor = tint;
                trail.endColor = new Color(tint.r, tint.g, tint.b, 0f);
                trail.time = TrailDuration;
                trail.emitting = _isBoosting;
            }
        }

        private void Apply()
        {
            var emission = _glow * (_glowStrength * EmissionPeak);
            foreach (var slot in _paintSlots)
            {
                slot.targetRenderer.GetPropertyBlock(_block, slot.materialIndex);
                _block.SetTexture(BaseMapProp, Texture2D.whiteTexture);
                _block.SetColor(BaseColorProp, _paint);
                _block.SetColor(EmissionColorProp, emission);
                slot.targetRenderer.SetPropertyBlock(_block, slot.materialIndex);
            }
        }
    }
}
