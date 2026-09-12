using Core.Utilities;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.UI.Abstracts
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseView : MonoBehaviour
    {
        [SerializeField, HideInInspector] private CanvasGroup canvasGroup;
        [SerializeField] private bool forceHide = true;

        private void OnValidate() => canvasGroup = GetComponent<CanvasGroup>();

        protected virtual void Awake() => Initialize();

        protected virtual void Initialize()
        {
            if (!forceHide) return;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public virtual void Show(float duration = 0.35f)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.GoFade(1f, duration, token: this.GetCancellationTokenOnDestroy());
        }

        public virtual void Hide(float duration = 0.25f)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.GoFade(0f, duration, token: this.GetCancellationTokenOnDestroy());
        }
    }
}
