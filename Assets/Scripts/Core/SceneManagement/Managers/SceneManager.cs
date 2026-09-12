using Core.SceneManagement.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using Core.UI.Managers;
// using Game.UI.Views;
using static UnityEngine.SceneManagement.SceneManager;

namespace Core.SceneManagement.Managers
{
    public class SceneManager : MonoBehaviour, ISceneService
    {
        [SerializeField] private UIManager uiManager;
        // private LoadingView _loadingView;
        public async void LoadScene(string sceneName, Action onComplete = null)
        {
            try
            {
                // _loadingView = uiManager.GetView<LoadingView>();
                var op = LoadSceneAsync(sceneName);
                await op;
                onComplete?.Invoke();
                // _loadingView?.Hide();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
}