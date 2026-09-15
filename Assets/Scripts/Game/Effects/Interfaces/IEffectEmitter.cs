using UnityEngine;

namespace Game.Effects.Interfaces
{
    public interface IEffectEmitter
    {
        public void Tint(Color color);
        public void SetRate(float scale);
        public void Burst(float scale);
        public void Run();
        public void Halt();
    }
}
