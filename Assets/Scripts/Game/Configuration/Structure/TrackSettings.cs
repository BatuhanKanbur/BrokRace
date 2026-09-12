using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Configuration.Structure
{
    [Serializable]
    public class TrackSettings
    {
        public float raceDistance = 1300f;
        public float roadWidth = 19f;
        public float laneSpacing = 2.2f;
        public Vector3[] shapePoints =
        {
            new(0f, 0f, 0f),
            new(0f, 0f, 220f),
            new(70f, 0f, 380f),
            new(70f, 0f, 560f),
            new(-60f, 0f, 700f),
            new(-180f, 0f, 760f),
            new(-260f, 0f, 900f),
            new(-200f, 0f, 1060f),
            new(-40f, 0f, 1120f),
            new(120f, 0f, 1180f),
            new(240f, 0f, 1300f),
            new(300f, 0f, 1460f)
        };
        public AssetReference roadMaterial;
        public AssetReference[] decorProps;
        public AssetReference finishGate;
    }
}
