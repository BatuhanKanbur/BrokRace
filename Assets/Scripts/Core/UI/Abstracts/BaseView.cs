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
        private void OnValidate()
        {
            if (!canvasGroup)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        protected virtual void Awake()
        {
            if (!canvasGroup)
                canvasGroup = GetComponent<CanvasGroup>();
            Initialize();
        }

        protected virtual void Initialize()
        {
            ForceHide();
        }

        public virtual void Show(float duration = 0.75f)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.GoFade(1f,duration,token : this.GetCancellationTokenOnDestroy());
        }
        public virtual void Hide(float duration = 0.5f)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.GoFade(0f,duration,token : this.GetCancellationTokenOnDestroy());
        }

        private void ForceHide()
        {
            if(!forceHide) return;
            if (!canvasGroup) return;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}