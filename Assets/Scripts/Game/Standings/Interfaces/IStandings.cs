using System;
using System.Collections.Generic;
using Game.Cars.Interfaces;

namespace Game.Standings.Interfaces
{
    public interface IStandings
    {
        public event Action<ICarProgress, int> OnRankChanged;
        public event Action<ICarProgress> OnCarFinished;
        public event Action OnAllFinished;
        public void Register(IReadOnlyList<ICar> cars);
        public void Refresh();
        public void ReportFinish(ICar car, float crossTime);
        public void Reset();
    }
}
