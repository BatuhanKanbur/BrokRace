using Game.Track.Interfaces;
using UnityEngine;
using static Game.Track.Constants.TrackConstants;

namespace Game.Track.Behaviours
{
    public class BakedTrack : MonoBehaviour, ITrackPath
    {
        [SerializeField] private Vector3[] centreLine;
        [SerializeField] private float[] milestones;

        public float Length => milestones[^1];

        public void Store(Vector3[] points, float[] travelled)
        {
            centreLine = points;
            milestones = travelled;
        }

        public Vector3 GetPosition(float distance, float laneOffset) =>
            Sample(distance) + Vector3.Cross(Vector3.up, GetForward(distance)) * laneOffset;

        public Vector3 GetForward(float distance)
        {
            var clamped = Mathf.Clamp(distance, 0f, Length);
            return (Interpolate(clamped + ForwardSampleSpan) - Interpolate(clamped - ForwardSampleSpan)).normalized;
        }

        public Quaternion GetRotation(float distance) => Quaternion.LookRotation(GetForward(distance));

        private Vector3 Sample(float distance)
        {
            if (distance >= Length) return centreLine[^1] + GetForward(Length) * (distance - Length);
            if (distance <= 0f) return centreLine[0] + GetForward(0f) * distance;
            return Interpolate(distance);
        }

        private Vector3 Interpolate(float distance)
        {
            var clamped = Mathf.Clamp(distance, 0f, Length);
            var index = IndexOf(clamped);
            var t = Mathf.InverseLerp(milestones[index], milestones[index + 1], clamped);
            return Vector3.Lerp(centreLine[index], centreLine[index + 1], t);
        }

        private int IndexOf(float distance)
        {
            var low = 0;
            var high = milestones.Length - 1;
            while (high - low > 1)
            {
                var mid = (low + high) >> 1;
                if (milestones[mid] <= distance) low = mid;
                else high = mid;
            }
            return low;
        }
    }
}
