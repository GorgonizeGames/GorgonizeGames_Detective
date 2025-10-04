using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Events;

namespace Game.Runtime.Core.Services
{
    public class SceneService : MonoBehaviour, ISceneService
    {
        [Inject] private IEventService _eventService;
        [Inject] private IAudioService _audioService;
        
        private string _currentSceneName;
        private bool _isLoading;
        
        private void Start()
        {
            Dependencies.Inject(this);
            _currentSceneName = SceneManager.GetActiveScene().name;
        }
        
        public async Task LoadSceneAsync(string sceneName, bool showLoadingScreen = true)
        {
            if (_isLoading)
            {
                Debug.LogWarning($"[SceneService] Already loading a scene");
                return;
            }
            
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneService] Scene name is null or empty");
                return;
            }
            
            _isLoading = true;
            
            try
            {
                _eventService?.Publish(new SceneLoadStartedEvent { SceneName = sceneName });
                _audioService?.StopMusic(0.5f);
                
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
                
                if (asyncLoad == null)
                {
                    Debug.LogError($"[SceneService] Failed to load scene: {sceneName}");
                    _isLoading = false;
                    return;
                }
                
                while (!asyncLoad.isDone)
                {
                    await Task.Yield();
                }
                
                _currentSceneName = sceneName;
                _eventService?.Publish(new SceneLoadedEvent { SceneName = sceneName });
            }
            catch (Exception e)
            {
                Debug.LogError($"[SceneService] Error loading scene '{sceneName}': {e.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }
        
        public async Task UnloadSceneAsync(string sceneName)
        {
            if (!IsSceneLoaded(sceneName)) return;
            
            try
            {
                AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
                if (asyncUnload == null) return;
                
                while (!asyncUnload.isDone)
                {
                    await Task.Yield();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SceneService] Error unloading scene '{sceneName}': {e.Message}");
            }
        }
        
        public async Task ReloadCurrentScene()
        {
            await LoadSceneAsync(_currentSceneName, true);
        }
        
        public string GetCurrentSceneName() => _currentSceneName;
        
        public bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName && scene.isLoaded) return true;
            }
            return false;
        }
    }
}