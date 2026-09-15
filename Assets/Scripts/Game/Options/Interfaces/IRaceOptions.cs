using Game.Ai.Enums;

namespace Game.Options.Interfaces
{
    public interface IRaceOptions
    {
        public RivalMode Rivals { get; }
        public bool IsBalancingEnabled { get; }
        public void SetRivals(RivalMode mode);
        public void SetBalancing(bool enabled);
    }
}
