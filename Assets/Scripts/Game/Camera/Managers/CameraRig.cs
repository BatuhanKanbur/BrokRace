using Game.Camera.Interfaces;
using UnityEngine;
using static Game.Camera.Constants.CameraConstants;

namespace Game.Camera.Managers
{
    public class CameraRig : ICameraRig
    {
        private Transform _target;
        private Vector3 _lastTargetPosition;
        private float _referenceSpeed;
        private float _roll;
        private Vector3 _departure;
        private float _showcaseTime;
        private bool _isShowcasing;

        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        public float FieldOfView { get; private set; }
        public float Speed { get; private set; }
        public float Intensity => Mathf.Clamp01((Speed - _referenceSpeed) / (_referenceSpeed * SpeedIntensitySpan));

        public void Follow(Transform target, float referenceSpeed)
        {
            _target = target;
            _referenceSpeed = referenceSpeed;
        }

        public void Showcase()
        {
            _showcaseTime = 0f;
            _isShowcasing = true;
            _departure = _target.InverseTransformPoint(Position);
        }

        public void Advance(float deltaTime)
        {
            var travel = _target.position - _lastTargetPosition;
            _lastTargetPosition = _target.position;
            Speed = deltaTime > 0f ? travel.magnitude / deltaTime : Speed;
            if (_isShowcasing)
            {
                AdvanceShowcase(deltaTime);
                return;
            }
            var lateral = Vector3.Dot(travel, Vector3.Cross(Vector3.up, _target.forward));
            var bank = Mathf.Clamp(-lateral * RollPerLateralMetre, -MaxRoll, MaxRoll);
            _roll = Mathf.Lerp(_roll, bank, RollLerp * deltaTime);
            Position = Vector3.Lerp(Position, Anchor(Speed), PositionLerp * deltaTime);
            Rotation = Quaternion.Slerp(Rotation, Aim() * Quaternion.Euler(0f, 0f, _roll),
                RotationLerp * deltaTime);
            FieldOfView = Mathf.Lerp(FieldOfView, Widened(Speed), FieldOfViewLerp * deltaTime);
        }

        public void Snap()
        {
            Speed = 0f;
            _roll = 0f;
            _showcaseTime = 0f;
            _isShowcasing = false;
            _lastTargetPosition = _target.position;
            Position = Anchor(0f);
            Rotation = Aim();
            FieldOfView = BaseFieldOfView;
        }

        private void AdvanceShowcase(float deltaTime)
        {
            _showcaseTime = Mathf.Min(_showcaseTime + deltaTime, ShowcaseDuration);
            var progress = Mathf.SmoothStep(0f, 1f, _showcaseTime / ShowcaseDuration);
            _roll = Mathf.Lerp(_roll, 0f, RollLerp * deltaTime);
            Position = _target.TransformPoint(Vector3.Lerp(_departure, Orbit(progress), progress));
            Rotation = Quaternion.Slerp(Rotation, Hero() * Quaternion.Euler(0f, 0f, _roll),
                ShowcaseRotationLerp * deltaTime);
            FieldOfView = Mathf.Lerp(FieldOfView, Mathf.Lerp(BaseFieldOfView, ShowcaseFieldOfView, progress),
                ShowcaseFieldOfViewLerp * deltaTime);
        }

        private static Vector3 Orbit(float progress) =>
            Quaternion.AngleAxis(ShowcaseSweep * progress, Vector3.up)
            * (Vector3.back * Mathf.Lerp(FollowDistance, ShowcaseDistance, progress))
            + Vector3.up * (Mathf.Lerp(FollowHeight, ShowcaseHeight, progress)
                            + Mathf.Sin(progress * Mathf.PI) * ShowcaseLift);

        private Quaternion Hero() =>
            Quaternion.LookRotation(_target.position + Vector3.up * ShowcaseAimHeight - Position);

        private Quaternion Aim() => Quaternion.LookRotation(_target.position + _target.forward * LookAhead - Position);

        private Vector3 Anchor(float speed) =>
            _target.position
            - _target.forward * (FollowDistance + speed * SpeedPullback)
            + Vector3.up * FollowHeight;

        private float Widened(float speed) =>
            Mathf.Min(BaseFieldOfView + Mathf.Max(0f, speed - _referenceSpeed) * FieldOfViewPerSpeed,
                MaxFieldOfView);
    }
}
