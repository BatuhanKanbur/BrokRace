namespace Core.Utilities
{
    public class DeterministicRandom
    {
        private const uint Fallback = 0x9E3779B9;
        private const uint MantissaMask = 0xFFFFFF;
        private const float MantissaScale = 1f / 0x1000000;

        private uint _state;

        public DeterministicRandom(int seed) => _state = seed == 0 ? Fallback : (uint)seed;

        public static DeterministicRandom Stream(int seed, int salt)
        {
            var mixed = (uint)seed * 747796405u + (uint)salt * 2891336453u;
            mixed ^= mixed >> 15;
            mixed *= 2246822519u;
            mixed ^= mixed >> 13;
            return new DeterministicRandom(unchecked((int)mixed));
        }

        public float NextFloat()
        {
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return (_state & MantissaMask) * MantissaScale;
        }

        public float Range(float min, float max) => min + (max - min) * NextFloat();

        public int Range(int min, int max) => min + (int)(NextFloat() * (max - min));

        public float Signed() => NextFloat() * 2f - 1f;

        public bool Chance(float probability) => NextFloat() < probability;
    }
}
