using System;
using System.Collections.Generic;
using Game.Cars.Interfaces;
using Game.Race.Structure;

namespace Game.Standings.Interfaces
{
    public interface IStandings
    {
        public event Action<ICarProgress, int> OnRankChanged;
        public event Action<ICarProgress> OnCarFinished;
        public void Register(IReadOnlyList<ICar> cars);
        public void Refresh();
        public void ReportCrossings(List<FinishCrossing> crossings, float stepStart);
        public void Reset();
    }
}
