using System;

namespace Core.SceneManagement.Interfaces
{
    public interface ISceneService
    {
        public void LoadScene(string sceneName, Action onComplete = null);
    }
}