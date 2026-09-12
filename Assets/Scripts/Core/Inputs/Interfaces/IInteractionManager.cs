using System;

namespace Core.Inputs.Interfaces
{
    public interface IInteractionManager
    {
        public event Action<IClickable> OnObjectClicked;
    }
}