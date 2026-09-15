using UnityEngine;

namespace Game.Effects.Interfaces
{
    public interface ICarEffectRig
    {
        public IEffectEmitter[] Thrusters { get; }
        public IEffectEmitter Sparks { get; }
        public IEffectEmitter Dust { get; }
        public IEffectEmitter Shockwave { get; }
        public IEffectEmitter Confetti { get; }
        public Light Flare { get; }
    }
}
