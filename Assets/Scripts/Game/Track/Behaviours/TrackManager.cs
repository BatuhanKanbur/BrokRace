using Cysharp.Threading.Tasks;
using Game.Configuration.Structure;
using Game.Track.Interfaces;
using PoolManager.Runtime;
using UnityEngine;
using Pool = PoolManager.Runtime.PoolManager;

namespace Game.Track.Behaviours
{
    public class TrackManager : MonoBehaviour, ITrackPath, ITrackBuilder
    {
        private ITrackPath _path;

        public float Length => _path.Length;

        public async UniTask Build(TrackSettings settings)
        {
            var instance = await Pool.GetObjectAsync(settings.trackPrefab)
                .SetParent(transform)
                .SetPosition(transform.position)
                .SetRotation(transform.rotation);
            _path = instance.GetComponent<ITrackPath>();
        }

        public Vector3 GetPosition(float distance, float laneOffset) => _path.GetPosition(distance, laneOffset);

        public Vector3 GetForward(float distance) => _path.GetForward(distance);

        public Quaternion GetRotation(float distance) => _path.GetRotation(distance);
    }
}
