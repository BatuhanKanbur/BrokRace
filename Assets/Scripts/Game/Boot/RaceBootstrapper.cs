using Core.DI.Managers;
using Core.UI.Abstracts;
using Core.UI.Interfaces;
using Core.UI.Managers;
using Cysharp.Threading.Tasks;
using Game.Camera.Behaviours;
using Game.Camera.Interfaces;
using Game.Configuration.Interfaces;
using Game.Configuration.Managers;
using Game.Configuration.Structure;
using Game.Input.Interfaces;
using Game.Input.Managers;
using Game.Manager.Interfaces;
using Game.Options.Interfaces;
using Game.Options.Managers;
using Game.Manager.Managers;
using Game.Race.Interfaces;
using Game.Race.Behaviours;
using Game.States;
using Game.Telemetry.Interfaces;
using Game.Telemetry.Logics;
using Game.Track.Behaviours;
using Game.Track.Interfaces;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Boot
{
    public class RaceBootstrapper : MonoBehaviour
    {
        [SerializeField] private AssetReferenceT<RaceConfig> configReference;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private BaseView[] views;
        [SerializeField] private TrackManager trackManager;
        [SerializeField] private RaceManager raceManager;
        [SerializeField] private RaceCamera raceCamera;
        [SerializeField] private GameManager gameManager;

        private void Awake() => Initialize().Forget();

        private async UniTaskVoid Initialize()
        {
            DiContainer.Clear();
            var configService = new RaceConfigManager(configReference);
            DiContainer.Register<IRaceConfigService>(configService);
            DiContainer.Register<IUIManager>(uiManager);
            DiContainer.Register<ITrackPath>(trackManager);
            DiContainer.Register<ITrackBuilder>(trackManager);
            DiContainer.Register<IRaceCamera>(raceCamera);
            DiContainer.Register<IRaceManager>(raceManager);
            DiContainer.Register<IGameManager>(gameManager);
            var boostInput = new ButtonBoostInput();
            DiContainer.Register<IBoostInputSource>(boostInput);
            DiContainer.Register<IBoostRequestSink>(boostInput);
            foreach (var view in views)
                uiManager.RegisterView(view);

            await configService.Load();
            DiContainer.Register<ITelemetryWriter>(new TelemetryFileWriter(configService.Config.telemetry));
            DiContainer.Register<IRaceOptions>(new RaceOptionsManager(configService.Config.balance.enabled));
            DiContainer.Inject(raceManager);
            gameManager.ChangeState(new RaceBootState(gameManager));
        }

        private void OnDestroy() => DiContainer.Clear();
    }
}
