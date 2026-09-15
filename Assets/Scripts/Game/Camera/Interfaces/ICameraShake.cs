using UnityEngine;

namespace Game.Camera.Interfaces
{
    public interface ICameraShake
    {
        public Vector3 Offset { get; }
        public float Roll { get; }
        public float FieldOfViewKick { get; }
        public void Punch(float strength);
        public void Advance(float deltaTime);
        public void Reset();
    }
}
