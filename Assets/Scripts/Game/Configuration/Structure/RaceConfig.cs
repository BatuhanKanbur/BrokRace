using UnityEngine;
using static Game.Race.Constants.RaceConstants;

namespace Game.Configuration.Structure
{
    [CreateAssetMenu(fileName = "RaceConfig", menuName = "BrokRace/Race Config")]
    public class RaceConfig : ScriptableObject
    {
        [Header("Race")]
        public RaceSettings race = new();
        [Header("Track")]
        public TrackSettings track = new();
        [Header("Boost Economy")]
        public BoostSettings boost = new();
        [Header("Rival AI")]
        public AiSettings ai = new();
        [Header("Rival Balancing")]
        public BalanceSettings balance = new();
        [Header("Telemetry")]
        public TelemetrySettings telemetry = new();
        [Header("Grid")]
        public CarEntry player = new();
        public RivalEntry[] rivals = new RivalEntry[RivalCount];

        private void OnValidate()
        {
            if (rivals.Length == RivalCount) return;
            var resized = new RivalEntry[RivalCount];
            for (var index = 0; index < RivalCount; index++)
                resized[index] = index < rivals.Length ? rivals[index] : new RivalEntry();
            rivals = resized;
        }
    }
}
