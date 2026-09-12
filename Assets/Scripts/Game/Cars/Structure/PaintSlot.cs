using System;
using UnityEngine;

namespace Game.Cars.Structure
{
    [Serializable]
    public struct PaintSlot
    {
        public Renderer targetRenderer;
        public int materialIndex;
    }
}
