using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Utilities
{
    public static class TweenExtensions
    {
        public static void GoFill(this Image image, float targetValue, float duration, AnimationCurve curve = null, CancellationToken token = default)
        {
            image.fillAmount = 0;
            GoFillAmountAsync(image, targetValue, duration, curve, token).Forget();
        }
        public static void GoFade(this CanvasGroup group, float targetAlpha, float duration, AnimationCurve curve = null, CancellationToken token = default)
        {
            FadeOperation(group, targetAlpha, duration, curve, token).Forget();
        }

        private static async UniTask GoFillAmountAsync(Image image, float targetValue, float duration, AnimationCurve curve = null, CancellationToken token = default)
        {
            var startValue = image.fillAmount;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                if (!image) return;
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var curvedT = curve?.Evaluate(t) ?? Mathf.SmoothStep(0f, 1f, t);
                image.fillAmount = Mathf.Lerp(startValue, targetValue, curvedT);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
            }
            if (image)
                image.fillAmount = targetValue;
        }

        private static async UniTaskVoid FadeOperation(CanvasGroup group, float targetAlpha, float duration, AnimationCurve curve, CancellationToken token)
        {
            if (!group) return;
            var startAlpha = group.alpha;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                if (token.IsCancellationRequested || group == null) return;
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var curvedT = curve?.Evaluate(t) ?? Mathf.SmoothStep(0f, 1f, t);
                group.alpha = Mathf.Lerp(startAlpha, targetAlpha, curvedT);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
                if (!group) return; 
            }
            if (group && !token.IsCancellationRequested)
            {
                group.alpha = targetAlpha;
            }
        }
    }
}