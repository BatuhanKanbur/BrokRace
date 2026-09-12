using UnityEngine;

namespace Game.Track.Interfaces
{
    public interface ITrackPath
    {
        public float Length { get; }
        public Vector3 GetPosition(float distance, float laneOffset);
        public Vector3 GetForward(float distance);
        public Quaternion GetRotation(float distance);
    }
}
