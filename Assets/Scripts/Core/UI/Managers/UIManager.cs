using System;
using System.Collections.Generic;
using Core.UI.Abstracts;
using Core.UI.Interfaces;
using UnityEngine;

namespace Core.UI.Managers
{
    public class UIManager : MonoBehaviour, IUIManager
    {
        private readonly Dictionary<Type, BaseView> _registeredViews = new();

        public void RegisterView(BaseView view)
        {
            if (!view) return;
            var type = view.GetType();
            _registeredViews[type] = view;
        }

        public void UnregisterView(BaseView view)
        {
            if (!view) return;
            var type = view.GetType();
            _registeredViews.Remove(type);
        }

        public T GetView<T>() where T : BaseView
        {
            var type = typeof(T);
            if (_registeredViews.TryGetValue(type, out var view))
            {
                return view as T;
            }
            return null;
        }

        public void Show<T>(bool hideOthers = false) where T : BaseView
        {
            var targetView = GetView<T>();
            if (!targetView) return;

            if (hideOthers)
            {
                foreach (var view in _registeredViews.Values)
                {
                    if (view != targetView && view != null) 
                    {
                        view.Hide();
                    }
                }
            }
            targetView.Show();
        }

        public void ClearAllViews()
        {
            _registeredViews.Clear();
        }
    }
}