using System;
using System.Collections.Generic;
using Game.Cars.Interfaces;
using Game.Standings.Interfaces;

namespace Game.Standings.Managers
{
    public class StandingsManager : IStandings, IRaceOrder
    {
        private readonly List<ICar> _sorted = new();
        private readonly List<ICarProgress> _view = new();
        private readonly Dictionary<ICarProgress, int> _ranks = new();

        public event Action<ICarProgress, int> OnRankChanged;
        public event Action<ICarProgress> OnCarFinished;

        public IReadOnlyList<ICarProgress> Order => _view;
        public ICarProgress Leader => _sorted[0];
        public ICarProgress Trailer => _sorted[^1];
        public int FinishedCount { get; private set; }

        public void Register(IReadOnlyList<ICar> cars)
        {
            _sorted.Clear();
            _sorted.AddRange(cars);
            Reset();
        }

        public void Refresh()
        {
            _sorted.Sort(Compare);
            _view.Clear();
            for (var index = 0; index < _sorted.Count; index++)
            {
                var car = _sorted[index];
                _view.Add(car);
                var rank = index + 1;
                if (_ranks[car] == rank) continue;
                _ranks[car] = rank;
                OnRankChanged?.Invoke(car, rank);
            }
        }

        public void ReportFinish(ICar car, float crossTime)
        {
            FinishedCount++;
            car.MarkFinished(FinishedCount, crossTime);
            OnCarFinished?.Invoke(car);
        }

        public int RankOf(ICarProgress car) => _ranks[car];

        public ICarProgress CarAhead(ICarProgress car)
        {
            var rank = _ranks[car];
            return rank > 1 ? _sorted[rank - 2] : null;
        }

        public ICarProgress CarBehind(ICarProgress car)
        {
            var rank = _ranks[car];
            return rank < _sorted.Count ? _sorted[rank] : null;
        }

        public void Reset()
        {
            FinishedCount = 0;
            _ranks.Clear();
            foreach (var car in _sorted)
                _ranks[car] = 0;
            Refresh();
        }

        private static int Compare(ICar left, ICar right)
        {
            if (left.HasFinished && right.HasFinished) return left.FinishOrder.CompareTo(right.FinishOrder);
            if (left.HasFinished != right.HasFinished) return left.HasFinished ? -1 : 1;
            return right.Distance.CompareTo(left.Distance);
        }
    }
}
