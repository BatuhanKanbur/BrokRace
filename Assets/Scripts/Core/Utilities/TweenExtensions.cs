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

        public static void GoTint(this Graphic graphic, Color targetColor, float duration,
            AnimationCurve curve = null, CancellationToken token = default) =>
            TintOperation(graphic, targetColor, duration, curve, token).Forget();

        public static void GoPunch(this RectTransform target, float targetScale, float duration,
            CancellationToken token = default) =>
            PunchOperation(target, targetScale, duration, token).Forget();

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

        private static async UniTaskVoid TintOperation(Graphic graphic, Color target, float duration,
            AnimationCurve curve, CancellationToken token)
        {
            var start = graphic.color;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                graphic.color = Color.Lerp(start, target, Shape(elapsed / duration, curve));
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            graphic.color = target;
        }

        private static async UniTaskVoid PunchOperation(RectTransform target, float targetScale, float duration,
            CancellationToken token)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var wave = Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI);
                target.localScale = Vector3.one * Mathf.LerpUnclamped(1f, targetScale, wave);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            target.localScale = Vector3.one;
        }

        private static float Shape(float progress, AnimationCurve curve)
        {
            var clamped = Mathf.Clamp01(progress);
            return curve?.Evaluate(clamped) ?? Mathf.SmoothStep(0f, 1f, clamped);
        }
    }
}
