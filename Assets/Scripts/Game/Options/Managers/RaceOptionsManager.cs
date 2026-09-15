using Game.Ai.Enums;
using Game.Options.Interfaces;

namespace Game.Options.Managers
{
    public class RaceOptionsManager : IRaceOptions
    {
        public RivalMode Rivals { get; private set; } = RivalMode.Normal;
        public bool IsBalancingEnabled { get; private set; }

        public RaceOptionsManager(bool balancingEnabled) => IsBalancingEnabled = balancingEnabled;

        public void SetRivals(RivalMode mode) => Rivals = mode;

        public void SetBalancing(bool enabled) => IsBalancingEnabled = enabled;
    }
}
