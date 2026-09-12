using UnityEngine;

namespace Game.Camera.Interfaces
{
    public interface IRaceCamera
    {
        public void Follow(Transform target);
        public void Present(float deltaTime);
        public void Snap();
    }
}
