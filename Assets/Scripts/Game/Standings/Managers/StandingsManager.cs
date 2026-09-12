using System;
using System.Collections.Generic;
using Game.Cars.Interfaces;
using Game.Race.Structure;
using Game.Standings.Interfaces;

namespace Game.Standings.Managers
{
    public class StandingsManager : IStandings, IRaceOrder
    {
        private static readonly Comparison<ICar> ByProgress = Compare;
        private static readonly Comparison<FinishCrossing> ByCrossing = CompareCrossings;

        private readonly List<ICar> _sorted = new();
        private readonly List<ICarProgress> _view = new();
        private readonly Dictionary<int, int> _ranks = new();

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
            _sorted.Sort(ByProgress);
            _view.Clear();
            for (var index = 0; index < _sorted.Count; index++)
            {
                var car = _sorted[index];
                _view.Add(car);
                var rank = index + 1;
                if (_ranks[car.Index] == rank) continue;
                _ranks[car.Index] = rank;
                OnRankChanged?.Invoke(car, rank);
            }
        }

        public void ReportCrossings(List<FinishCrossing> crossings, float stepStart)
        {
            crossings.Sort(ByCrossing);
            foreach (var crossing in crossings)
            {
                FinishedCount++;
                crossing.Car.MarkFinished(FinishedCount, stepStart + crossing.Offset);
                OnCarFinished?.Invoke(crossing.Car);
            }
            crossings.Clear();
            Refresh();
        }

        public void Reset()
        {
            FinishedCount = 0;
            _sorted.Sort(ByProgress);
            _view.Clear();
            for (var index = 0; index < _sorted.Count; index++)
            {
                _view.Add(_sorted[index]);
                _ranks[_sorted[index].Index] = index + 1;
            }
        }

        public int RankOf(ICarProgress car) => _ranks[car.Index];

        public ICarProgress CarAhead(ICarProgress car)
        {
            var rank = _ranks[car.Index];
            return rank > 1 ? _sorted[rank - 2] : null;
        }

        public ICarProgress CarBehind(ICarProgress car)
        {
            var rank = _ranks[car.Index];
            return rank < _sorted.Count ? _sorted[rank] : null;
        }

        private static int Compare(ICar left, ICar right)
        {
            if (left.HasFinished && right.HasFinished) return left.FinishOrder.CompareTo(right.FinishOrder);
            if (left.HasFinished != right.HasFinished) return left.HasFinished ? -1 : 1;
            var byDistance = right.Distance.CompareTo(left.Distance);
            return byDistance != 0 ? byDistance : left.Index.CompareTo(right.Index);
        }

        private static int CompareCrossings(FinishCrossing left, FinishCrossing right)
        {
            var byOffset = left.Offset.CompareTo(right.Offset);
            if (byOffset != 0) return byOffset;
            var bySpeed = right.Car.Speed.CompareTo(left.Car.Speed);
            return bySpeed != 0 ? bySpeed : left.Car.Index.CompareTo(right.Car.Index);
        }
    }
}
