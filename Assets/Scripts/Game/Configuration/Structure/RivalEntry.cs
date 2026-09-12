using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Configuration.Structure
{
    [Serializable]
    public class RivalEntry
    {
        public string displayName = "Rival";
        public AssetReference prefab;
        public Color paint = Color.white;
        public AiProfile profile = new();
    }
}
