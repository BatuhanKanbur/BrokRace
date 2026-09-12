using System.Collections.Generic;
using Game.Cars.Interfaces;

namespace Game.Race.Interfaces
{
    public interface IRaceState
    {
        public int Seed { get; }
        public float Time { get; }
        public float RaceDistance { get; }
        public float LogicStep { get; }
        public bool IsRunning { get; }
        public IReadOnlyList<ICar> Cars { get; }
        public ICar Player { get; }
    }
}
