using Core.DI.Managers;
using UnityEngine;

namespace Core.DI.Abstracts
{
    public abstract class SceneContext : MonoBehaviour
    {
        protected virtual void Awake()
        {
            DiContainer.Inject(this);
            RegisterSceneDependencies();
        }

        protected abstract void RegisterSceneDependencies();
    }
}