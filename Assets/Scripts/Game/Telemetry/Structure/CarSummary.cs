using System;

namespace Game.Telemetry.Structure
{
    [Serializable]
    public class CarSummary
    {
        public int carIndex;
        public string name;
        public string profile;
        public bool isPlayer;
        public int finishOrder;
        public float finishTime;
        public int acceptedBoosts;
        public int rejectedBoosts;
        public float energySpent;
        public float boostedSeconds;
        public float averageSpeed;
    }
}
