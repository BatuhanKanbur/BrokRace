namespace Game.Ai.Interfaces
{
    public interface IAiDriver
    {
        public int CarIndex { get; }
        public string ProfileName { get; }
        public string StateLabel { get; }
        public void Step(float stepTime);
        public void Reset();
    }
}
