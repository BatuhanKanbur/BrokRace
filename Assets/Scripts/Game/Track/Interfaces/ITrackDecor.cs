using Cysharp.Threading.Tasks;
using Game.Configuration.Structure;

namespace Game.Track.Interfaces
{
    public interface ITrackDecor
    {
        public UniTask Decorate(ITrackPath path, TrackSettings settings);
        public void Clear();
    }
}
