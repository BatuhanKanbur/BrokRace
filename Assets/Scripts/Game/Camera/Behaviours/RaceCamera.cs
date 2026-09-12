using Game.Camera.Interfaces;
using UnityEngine;
using static Game.Camera.Constants.CameraConstants;

namespace Game.Camera.Behaviours
{
    public class RaceCamera : MonoBehaviour, IRaceCamera
    {
        private UnityEngine.Camera _lens;
        private Transform _target;
        private Vector3 _lastTargetPosition;
        private float _referenceSpeed;

        private void Awake() => _lens = GetComponent<UnityEngine.Camera>();

        public void Follow(Transform target, float referenceSpeed)
        {
            _target = target;
            _referenceSpeed = referenceSpeed;
            _lastTargetPosition = target.position;
            Snap();
        }

        public void Present(float deltaTime)
        {
            var speed = deltaTime > 0f ? (_target.position - _lastTargetPosition).magnitude / deltaTime : 0f;
            _lastTargetPosition = _target.position;
            var desired = Anchor(speed);
            transform.position = Vector3.Lerp(transform.position, desired, PositionLerp * deltaTime);
            var focus = _target.position + _target.forward * LookAhead;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(focus - transform.position), RotationLerp * deltaTime);
            var widened = BaseFieldOfView + Mathf.Max(0f, speed - _referenceSpeed) * FieldOfViewPerSpeed;
            _lens.fieldOfView = Mathf.Lerp(_lens.fieldOfView, Mathf.Min(widened, MaxFieldOfView),
                FieldOfViewLerp * deltaTime);
        }

        public void Snap()
        {
            transform.position = Anchor(0f);
            transform.LookAt(_target.position + _target.forward * LookAhead);
            _lens.fieldOfView = BaseFieldOfView;
        }

        private Vector3 Anchor(float speed) =>
            _target.position
            - _target.forward * (FollowDistance + speed * SpeedPullback)
            + Vector3.up * FollowHeight;
    }
}
