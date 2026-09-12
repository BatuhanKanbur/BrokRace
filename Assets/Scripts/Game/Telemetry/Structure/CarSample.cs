using System;

namespace Game.Telemetry.Structure
{
    [Serializable]
    public struct CarSample
    {
        public float time;
        public int carIndex;
        public bool isPlayer;
        public int rank;
        public float distance;
        public float speed;
        public int boostLevel;
        public float energy;
        public float balanceScale;
        public float assistDistance;
        public float boostDistance;
        public float gapToPlayer;
        public float gapToLeader;
        public string aiState;
    }
}
