using Cysharp.Threading.Tasks;
using Game.Configuration.Structure;
using Game.Track.Interfaces;
using Game.Track.Logics;
using Game.Track.Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Track.Behaviours
{
    public class TrackManager : MonoBehaviour, ITrackPath, ITrackBuilder
    {
        [SerializeField] private MeshFilter roadFilter;
        [SerializeField] private MeshRenderer roadRenderer;
        [SerializeField] private Transform decorRoot;

        private readonly SplinePath _path = new();
        private ITrackDecor _decor;
        private AsyncOperationHandle<Material> _roadMaterialHandle;

        public float Length => _path.Length;

        private void Awake() => _decor = new TrackDecorator(decorRoot);

        public async UniTask Build(TrackSettings settings)
        {
            _path.Build(settings.shapePoints, settings.raceDistance);
            roadFilter.sharedMesh = RoadMeshBuilder.Build(_path.Points, settings.roadWidth);
            if (!_roadMaterialHandle.IsValid())
                _roadMaterialHandle = settings.roadMaterial.LoadAssetAsync<Material>();
            roadRenderer.sharedMaterial = await _roadMaterialHandle.ToUniTask();
            await _decor.Decorate(this, settings);
        }

        public Vector3 GetPosition(float distance, float laneOffset) =>
            _path.GetPosition(distance) + Vector3.Cross(Vector3.up, _path.GetForward(distance)) * laneOffset;

        public Vector3 GetForward(float distance) => _path.GetForward(distance);

        public Quaternion GetRotation(float distance) => Quaternion.LookRotation(_path.GetForward(distance));

        private void OnDestroy()
        {
            _decor.Clear();
            if (_roadMaterialHandle.IsValid())
                Addressables.Release(_roadMaterialHandle);
        }
    }
}
