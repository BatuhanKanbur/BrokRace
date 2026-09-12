using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Configuration.Structure
{
    [Serializable]
    public class CarEntry
    {
        public string displayName = "Player";
        public AssetReference prefab;
        public Color paint = Color.white;
    }
}
