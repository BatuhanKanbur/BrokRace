using Game.Camera.Interfaces;
using Game.Camera.Managers;
using Game.Effects.Interfaces;
using Game.Effects.Managers;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Camera.Behaviours
{
    public class RaceCamera : MonoBehaviour, IRaceCamera
    {
        [SerializeField] private UnityEngine.Camera lens;
        [SerializeField] private Volume volume;

        private readonly ICameraRig _rig = new CameraRig();
        private readonly ICameraShake _shake = new CameraShake();

        private IScreenEffects _screen;

        private void Awake() => _screen = new ScreenEffects(volume);

        public void Follow(Transform target, float referenceSpeed)
        {
            _rig.Follow(target, referenceSpeed);
            Snap();
        }

        public void Showcase() => _rig.Showcase();

        public void Present(float deltaTime)
        {
            _rig.Advance(deltaTime);
            _shake.Advance(deltaTime);
            transform.SetPositionAndRotation(_rig.Position + _rig.Rotation * _shake.Offset,
                _rig.Rotation * Quaternion.Euler(0f, 0f, _shake.Roll));
            lens.fieldOfView = _rig.FieldOfView + _shake.FieldOfViewKick;
            _screen.Present(_rig.Intensity, deltaTime);
        }

        public void Punch(float strength)
        {
            _shake.Punch(strength);
            _screen.Punch(strength);
        }

        public void Snap()
        {
            _rig.Snap();
            _shake.Reset();
            _screen.Reset();
            transform.SetPositionAndRotation(_rig.Position, _rig.Rotation);
            lens.fieldOfView = _rig.FieldOfView;
        }
    }
}
