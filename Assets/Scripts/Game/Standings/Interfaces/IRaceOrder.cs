using System.Collections.Generic;
using Game.Cars.Interfaces;

namespace Game.Standings.Interfaces
{
    public interface IRaceOrder
    {
        public IReadOnlyList<ICarProgress> Order { get; }
        public ICarProgress Leader { get; }
        public ICarProgress Trailer { get; }
        public int FinishedCount { get; }
        public int RankOf(ICarProgress car);
        public ICarProgress CarAhead(ICarProgress car);
        public ICarProgress CarBehind(ICarProgress car);
    }
}
