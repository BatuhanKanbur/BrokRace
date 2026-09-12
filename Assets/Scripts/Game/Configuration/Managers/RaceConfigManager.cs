using Cysharp.Threading.Tasks;
using Game.Configuration.Interfaces;
using Game.Configuration.Structure;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Configuration.Managers
{
    public class RaceConfigManager : IRaceConfigService
    {
        private readonly AssetReferenceT<RaceConfig> _reference;
        private AsyncOperationHandle<RaceConfig> _handle;

        public RaceConfig Config { get; private set; }

        public RaceConfigManager(AssetReferenceT<RaceConfig> reference) => _reference = reference;

        public async UniTask Load()
        {
            if (Config) return;
            _handle = _reference.LoadAssetAsync<RaceConfig>();
            Config = await _handle.ToUniTask();
        }

        public void Unload()
        {
            Config = null;
            if (_handle.IsValid())
                Addressables.Release(_handle);
        }
    }
}
