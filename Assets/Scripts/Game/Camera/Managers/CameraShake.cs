using Game.Camera.Interfaces;
using UnityEngine;
using static Game.Camera.Constants.CameraConstants;

namespace Game.Camera.Managers
{
    public class CameraShake : ICameraShake
    {
        private float _trauma;
        private float _time;

        public Vector3 Offset { get; private set; }
        public float Roll { get; private set; }
        public float FieldOfViewKick { get; private set; }

        public void Punch(float strength) =>
            _trauma = Mathf.Min(MaxTrauma, _trauma + TraumaPerPunch * strength);

        public void Advance(float deltaTime)
        {
            _time += deltaTime;
            _trauma = Mathf.Max(0f, _trauma - TraumaDecay * deltaTime);
            var shake = _trauma * _trauma;
            Offset = new Vector3(Wave(ShakeSeedX), Wave(ShakeSeedY), 0f) * (ShakeAmplitude * shake);
            Roll = Wave(ShakeSeedRoll) * ShakeRoll * shake;
            FieldOfViewKick = ShakeFieldOfViewKick * shake;
        }

        public void Reset()
        {
            _trauma = 0f;
            Offset = Vector3.zero;
            Roll = 0f;
            FieldOfViewKick = 0f;
        }

        private float Wave(float seed) =>
            (Mathf.PerlinNoise(seed, _time * ShakeFrequency) - NoiseCentre) * NoiseSpan;
    }
}
