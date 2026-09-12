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

        public void RegisterView(BaseView view) => _registeredViews[view.GetType()] = view;

        public T GetView<T>() where T : BaseView => (T)_registeredViews[typeof(T)];

        public void Show<T>(bool hideOthers = false) where T : BaseView
        {
            var target = GetView<T>();
            if (hideOthers)
            {
                foreach (var view in _registeredViews.Values)
                    if (view != target)
                        view.Hide();
            }
            target.Show();
        }

        public void ClearAllViews() => _registeredViews.Clear();
    }
}
