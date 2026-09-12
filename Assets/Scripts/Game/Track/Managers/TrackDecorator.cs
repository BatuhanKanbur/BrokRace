using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Configuration.Structure;
using Game.Track.Interfaces;
using PoolManager.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Pool = PoolManager.Runtime.PoolManager;
using static Game.Track.Constants.TrackConstants;

namespace Game.Track.Managers
{
    public class TrackDecorator : ITrackDecor
    {
        private readonly Transform _root;
        private readonly List<GameObject> _spawned = new();

        public TrackDecorator(Transform root) => _root = root;

        public async UniTask Decorate(ITrackPath path, TrackSettings settings)
        {
            Clear();
            var side = settings.roadWidth * 0.5f + ShoulderWidth + DecorSideMargin;
            var slot = 0;
            for (var distance = StartLineOffset; distance < path.Length; distance += DecorSpacing)
            {
                var prop = settings.decorProps[slot % settings.decorProps.Length];
                await Place(prop, path.GetPosition(distance, side), path.GetForward(distance));
                await Place(prop, path.GetPosition(distance, -side), path.GetForward(distance));
                slot++;
            }
            await Place(settings.finishGate, path.GetPosition(path.Length, 0f), path.GetForward(path.Length));
        }

        public void Clear()
        {
            foreach (var instance in _spawned)
                instance.SetActive(false);
            _spawned.Clear();
        }

        private async UniTask Place(AssetReference reference, Vector3 position, Vector3 forward)
        {
            var instance = await Pool.GetObjectAsync(reference)
                .SetParent(_root)
                .SetPosition(position)
                .SetRotation(Quaternion.LookRotation(forward));
            _spawned.Add(instance);
        }
    }
}
