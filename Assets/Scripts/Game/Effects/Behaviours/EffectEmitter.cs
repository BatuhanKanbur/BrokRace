using Game.Effects.Interfaces;
using UnityEngine;

namespace Game.Effects.Behaviours
{
    public class EffectEmitter : MonoBehaviour, IEffectEmitter
    {
        private ParticleSystem[] _systems;
        private float[] _authoredRates;

        private void Awake()
        {
            _systems = GetComponentsInChildren<ParticleSystem>(true);
            _authoredRates = new float[_systems.Length];
            for (var index = 0; index < _systems.Length; index++)
                _authoredRates[index] = _systems[index].emission.rateOverTimeMultiplier;
        }

        public void Tint(Color color)
        {
            foreach (var system in _systems)
            {
                var main = system.main;
                main.startColor = color;
            }
        }

        public void SetRate(float scale)
        {
            for (var index = 0; index < _systems.Length; index++)
            {
                var emission = _systems[index].emission;
                emission.rateOverTimeMultiplier = _authoredRates[index] * scale;
            }
        }

        public void Burst(float scale)
        {
            transform.localScale = Vector3.one * scale;
            foreach (var system in _systems)
                system.Play(false);
        }

        public void Run()
        {
            foreach (var system in _systems)
                system.Play(false);
        }

        public void Halt()
        {
            foreach (var system in _systems)
                system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
