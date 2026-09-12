using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Utilities
{
    public static class TweenExtensions
    {
        public static void GoFill(this Image image, float targetValue, float duration,
            AnimationCurve curve = null, CancellationToken token = default) =>
            FillOperation(image, targetValue, duration, curve, token).Forget();

        public static void GoFade(this CanvasGroup group, float targetAlpha, float duration,
            AnimationCurve curve = null, CancellationToken token = default) =>
            FadeOperation(group, targetAlpha, duration, curve, token).Forget();

        private static async UniTaskVoid FillOperation(Image image, float target, float duration,
            AnimationCurve curve, CancellationToken token)
        {
            var start = image.fillAmount;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                image.fillAmount = Mathf.Lerp(start, target, Shape(elapsed / duration, curve));
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            image.fillAmount = target;
        }

        private static async UniTaskVoid FadeOperation(CanvasGroup group, float target, float duration,
            AnimationCurve curve, CancellationToken token)
        {
            var start = group.alpha;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                group.alpha = Mathf.Lerp(start, target, Shape(elapsed / duration, curve));
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            group.alpha = target;
        }

        private static float Shape(float progress, AnimationCurve curve)
        {
            var clamped = Mathf.Clamp01(progress);
            return curve?.Evaluate(clamped) ?? Mathf.SmoothStep(0f, 1f, clamped);
        }
    }
}
