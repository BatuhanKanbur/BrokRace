using UnityEngine;

namespace Game.Camera.Interfaces
{
    public interface ICameraRig
    {
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public float FieldOfView { get; }
        public float Speed { get; }
        public float Intensity { get; }
        public void Follow(Transform target, float referenceSpeed);
        public void Showcase();
        public void Advance(float deltaTime);
        public void Snap();
    }
}
