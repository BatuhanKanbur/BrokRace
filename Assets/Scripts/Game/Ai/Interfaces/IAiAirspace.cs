namespace Game.Ai.Interfaces
{
    public interface IAiAirspace
    {
        public bool TryClaim(float raceTime);
        public void Reset();
    }
}
