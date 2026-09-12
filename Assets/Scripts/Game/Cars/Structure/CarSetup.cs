using Game.Configuration.Structure;
using UnityEngine;

namespace Game.Cars.Structure
{
    public readonly struct CarSetup
    {
        public int Index { get; }
        public string DisplayName { get; }
        public bool IsPlayer { get; }
        public Color Paint { get; }
        public float LaneOffset { get; }
        public float NaturalSpeed { get; }
        public float FinishDistance { get; }
        public BoostSettings Boost { get; }

        public CarSetup(int index, string displayName, bool isPlayer, Color paint, float laneOffset,
            float naturalSpeed, float finishDistance, BoostSettings boost)
        {
            Index = index;
            DisplayName = displayName;
            IsPlayer = isPlayer;
            Paint = paint;
            LaneOffset = laneOffset;
            NaturalSpeed = naturalSpeed;
            FinishDistance = finishDistance;
            Boost = boost;
        }
    }
}
