using Game.Cars.Interfaces;

namespace Game.Race.Structure
{
    public readonly struct FinishCrossing
    {
        public ICar Car { get; }
        public float Offset { get; }

        public FinishCrossing(ICar car, float offset)
        {
            Car = car;
            Offset = offset;
        }
    }
}
