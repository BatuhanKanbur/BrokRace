using Cysharp.Threading.Tasks;
using Game.Configuration.Structure;

namespace Game.Configuration.Interfaces
{
    public interface IRaceConfigService
    {
        public RaceConfig Config { get; }
        public UniTask Load();
        public void Unload();
    }
}
