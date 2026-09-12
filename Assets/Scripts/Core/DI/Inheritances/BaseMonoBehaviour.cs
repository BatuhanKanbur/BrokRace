using Core.DI.Managers;
using UnityEngine;

namespace Core.DI.Inheritances
{
    public abstract class BaseMonoBehaviour : MonoBehaviour
    {
        protected virtual void Awake()
        {
            DiContainer.Inject(this);
        }
    }
}