namespace Game.Cars.Structure
{
    public readonly struct MotionStep
    {
        public float Travelled { get; }
        public float CrossOffset { get; }
        public bool HasCrossed => CrossOffset >= 0f;

        public MotionStep(float travelled, float crossOffset)
        {
            Travelled = travelled;
            CrossOffset = crossOffset;
        }
    }
}
