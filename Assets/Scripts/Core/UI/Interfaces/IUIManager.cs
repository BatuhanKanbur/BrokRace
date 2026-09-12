using Core.UI.Abstracts;

namespace Core.UI.Interfaces
{
    public interface IUIManager
    {
        public void RegisterView(BaseView view);
        public T GetView<T>() where T : BaseView;
        public void Show<T>(bool hideOthers = false) where T : BaseView;
        public void ClearAllViews();
    }
}