using Game.Ai.Structure;

namespace Game.Ai.Interfaces
{
    public interface IAiDriver
    {
        public int CarIndex { get; }
        public string ProfileName { get; }
        public AiDecision LastDecision { get; }
        public int Decide();
        public void Reset();
    }
}
