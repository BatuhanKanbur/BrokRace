using System.Collections.Generic;
using UnityEngine;
using static Game.Track.Constants.TrackConstants;

namespace Game.Track.Logics
{
    public class SplinePath
    {
        private readonly List<Vector3> _points = new();
        private readonly List<float> _distances = new();

        public float Length => _distances[^1];
        public IReadOnlyList<Vector3> Points => _points;

        public void Build(IReadOnlyList<Vector3> shapePoints, float targetLength)
        {
            _points.Clear();
            _distances.Clear();
            var steps = (shapePoints.Count - 1) * SamplesPerSegment;
            for (var step = 0; step <= steps; step++)
                _points.Add(Evaluate(shapePoints, step / (float)SamplesPerSegment));
            var scale = targetLength / MeasureLength();
            for (var index = 0; index < _points.Count; index++)
            {
                var point = _points[index];
                _points[index] = new Vector3(point.x * scale, point.y, point.z * scale);
            }
            Accumulate();
        }

        public Vector3 GetPosition(float distance)
        {
            if (distance >= Length) return _points[^1] + GetForward(Length) * (distance - Length);
            if (distance <= 0f) return _points[0] + GetForward(0f) * distance;
            var index = IndexOf(distance);
            var t = Mathf.InverseLerp(_distances[index], _distances[index + 1], distance);
            return Vector3.Lerp(_points[index], _points[index + 1], t);
        }

        public Vector3 GetForward(float distance)
        {
            var clamped = Mathf.Clamp(distance, 0f, Length);
            var back = SegmentPosition(clamped - ForwardSampleSpan);
            var ahead = SegmentPosition(clamped + ForwardSampleSpan);
            return (ahead - back).normalized;
        }

        private Vector3 SegmentPosition(float distance)
        {
            var clamped = Mathf.Clamp(distance, 0f, Length);
            var index = IndexOf(clamped);
            var t = Mathf.InverseLerp(_distances[index], _distances[index + 1], clamped);
            return Vector3.Lerp(_points[index], _points[index + 1], t);
        }

        private int IndexOf(float distance)
        {
            var low = 0;
            var high = _distances.Count - 1;
            while (high - low > 1)
            {
                var mid = (low + high) >> 1;
                if (_distances[mid] <= distance) low = mid;
                else high = mid;
            }
            return low;
        }

        private float MeasureLength()
        {
            var total = 0f;
            for (var index = 1; index < _points.Count; index++)
                total += Vector3.Distance(_points[index - 1], _points[index]);
            return total;
        }

        private void Accumulate()
        {
            var travelled = 0f;
            _distances.Add(0f);
            for (var index = 1; index < _points.Count; index++)
            {
                travelled += Vector3.Distance(_points[index - 1], _points[index]);
                _distances.Add(travelled);
            }
        }

        private static Vector3 Evaluate(IReadOnlyList<Vector3> shapePoints, float t)
        {
            var last = shapePoints.Count - 1;
            var segment = Mathf.Min((int)t, last - 1);
            var local = t - segment;
            return CatmullRom(
                shapePoints[Mathf.Max(segment - 1, 0)],
                shapePoints[segment],
                shapePoints[segment + 1],
                shapePoints[Mathf.Min(segment + 2, last)],
                local);
        }

        private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            return 0.5f * (2f * p1
                           + (p2 - p0) * t
                           + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2
                           + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }
    }
}
