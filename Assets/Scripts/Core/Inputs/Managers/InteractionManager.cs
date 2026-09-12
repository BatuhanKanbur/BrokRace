using System;
using Core.DI.Attributes;
using Core.DI.Managers;
using Core.Inputs.Interfaces;
using UnityEngine;

namespace Core.Inputs.Managers
{
    public class InteractionManager : MonoBehaviour, IInteractionManager
    {
        [SerializeField] private LayerMask clickableLayer;
        [SerializeField] private Camera mainCamera;

        [Inject] private IInputReader _inputReader;
        public event Action<IClickable> OnObjectClicked;

        private void Awake()
        {
            DiContainer.Register<IInteractionManager>(this);
        }

        private void Start()
        {
            _inputReader.OnTap += HandleTap;
            _inputReader.Enable();
        }

        private void HandleTap()
        {
            var screenPos = _inputReader.GetPointerPosition();
            var ray = mainCamera.ScreenPointToRay(screenPos);
            if (!Physics.Raycast(ray, out var hit, 100f, clickableLayer)) return;
            if (hit.collider.TryGetComponent<IClickable>(out var clickable))
            {
                OnObjectClicked?.Invoke(clickable);
            }
        }

        private void OnDestroy()
        {
            if (_inputReader != null)
            {
                _inputReader.OnTap -= HandleTap;
            }
        }
    }
}