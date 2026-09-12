using UnityEngine;

namespace Game.Simulation.Structure
{
    public readonly struct BoostCheck
    {
        public int Level { get; }
        public float StepTime { get; }
        public float BaseSpeed { get; }
        public float ExpectedWindowDistance { get; }
        public float MeasuredWindowDistance { get; }
        public float ExpectedExtra { get; }
        public float MeasuredExtra { get; }
        public float BoostedSeconds { get; }
        public float TotalDistance { get; }

        public float DeviationPercent => ExpectedWindowDistance > 0f
            ? Mathf.Abs(MeasuredWindowDistance - ExpectedWindowDistance) / ExpectedWindowDistance * 100f
            : 0f;

        public BoostCheck(int level, float stepTime, float baseSpeed, float expectedWindowDistance,
            float measuredWindowDistance, float expectedExtra, float measuredExtra, float boostedSeconds,
            float totalDistance)
        {
            Level = level;
            StepTime = stepTime;
            BaseSpeed = baseSpeed;
            ExpectedWindowDistance = expectedWindowDistance;
            MeasuredWindowDistance = measuredWindowDistance;
            ExpectedExtra = expectedExtra;
            MeasuredExtra = measuredExtra;
            BoostedSeconds = boostedSeconds;
            TotalDistance = totalDistance;
        }
    }
}
