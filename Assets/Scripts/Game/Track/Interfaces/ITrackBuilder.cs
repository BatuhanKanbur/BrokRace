using Cysharp.Threading.Tasks;
using Game.Configuration.Structure;

namespace Game.Track.Interfaces
{
    public interface ITrackBuilder
    {
        public UniTask Build(TrackSettings settings);
    }
}
