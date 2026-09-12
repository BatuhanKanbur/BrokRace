using UnityEngine;

namespace Game.Camera.Interfaces
{
    public interface IRaceCamera
    {
        public void Follow(Transform target, float referenceSpeed);
        public void Present(float deltaTime);
        public void Snap();
    }
}
