using Game.Effects.Interfaces;
using UnityEngine;

namespace Game.Effects.Behaviours
{
    public class CarEffectRig : MonoBehaviour, ICarEffectRig
    {
        [SerializeField] private EffectEmitter[] thrusters;
        [SerializeField] private EffectEmitter sparks;
        [SerializeField] private EffectEmitter dust;
        [SerializeField] private EffectEmitter shockwave;
        [SerializeField] private EffectEmitter confetti;
        [SerializeField] private Light flare;

        public IEffectEmitter[] Thrusters => thrusters;
        public IEffectEmitter Sparks => sparks;
        public IEffectEmitter Dust => dust;
        public IEffectEmitter Shockwave => shockwave;
        public IEffectEmitter Confetti => confetti;
        public Light Flare => flare;
    }
}
