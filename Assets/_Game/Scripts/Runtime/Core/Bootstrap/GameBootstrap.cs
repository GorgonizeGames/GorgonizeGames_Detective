using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Services;
using Game.Runtime.UI.Windows;
using Game.Runtime.Investigation;
using Game.Runtime.Hacking;
using Game.Runtime.Core.Services.Factories;
using Game.Runtime.Hacking.Services;

namespace Game.Runtime.Core.Bootstrap
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Core Services")]
        [SerializeField] private EventService _eventService;
        [SerializeField] private AudioService _audioService;
        [SerializeField] private SceneService _sceneService;
        [SerializeField] private InputService _inputService;
        
        [Header("UI Services")]
        [SerializeField] private WindowManager _windowManager;
        
        [Header("Game Services")]
        [SerializeField] private EvidenceService _evidenceService;
        [SerializeField] private ClueService _clueService;
        [SerializeField] private HackingService _hackingService;
        
        [Header("Settings")]
        [SerializeField] private GameSettings _gameSettings;
        
        [Header("Save System Settings")]
        [SerializeField] private bool _forceSteamInEditor = false;
        
        [Header("Debug")]
        [SerializeField] private bool _enableDebugLogs = true;
        
        private ISaveService _saveService;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            CreateSaveService();
            RegisterAllServices();
            ConfigureApplication();
            
            if (_enableDebugLogs)
            {
                Debug.Log($"[GameBootstrap] ✅ Bootstrap completed! {Dependencies.Container.ServiceCount} services registered");
            }
        }
        
        private void CreateSaveService()
        {
            _saveService = SaveServiceFactory.CreateSaveService(gameObject, _forceSteamInEditor);
            
            if (_saveService != null)
            {
                Debug.Log($"[GameBootstrap] Save service: {_saveService.GetType().Name}");
                Debug.Log($"[GameBootstrap] Platform: {_saveService.CurrentPlatform}");
                Debug.Log($"[GameBootstrap] Cloud: {_saveService.IsCloudEnabled}");
            }
        }
        
        private void RegisterAllServices()
        {
            var container = Dependencies.Container;
            
            // Core Services
            if (_eventService != null)
            {
                container.Register<IEventService>(_eventService);
                LogRegistration("EventService");
            }
            else Debug.LogError("[GameBootstrap] ❌ EventService missing!");
            
            if (_saveService != null)
            {
                container.Register<ISaveService>(_saveService);
                LogRegistration($"SaveService ({_saveService.CurrentPlatform})");
            }
            else Debug.LogError("[GameBootstrap] ❌ SaveService missing!");
            
            if (_audioService != null)
            {
                container.Register<IAudioService>(_audioService);
                LogRegistration("AudioService");
            }
            else Debug.LogError("[GameBootstrap] ❌ AudioService missing!");
            
            if (_sceneService != null)
            {
                container.Register<ISceneService>(_sceneService);
                LogRegistration("SceneService");
            }
            else Debug.LogError("[GameBootstrap] ❌ SceneService missing!");
            
            if (_inputService != null)
            {
                container.Register<IInputService>(_inputService);
                LogRegistration("InputService");
            }
            else Debug.LogError("[GameBootstrap] ❌ InputService missing!");
            
            // UI Services
            if (_windowManager != null)
            {
                container.Register<IWindowManager>(_windowManager);
                LogRegistration("WindowManager");
            }
            else Debug.LogError("[GameBootstrap] ❌ WindowManager missing!");
            
            // Game Services
            if (_evidenceService != null)
            {
                container.Register<IEvidenceService>(_evidenceService);
                LogRegistration("EvidenceService");
            }
            else Debug.LogError("[GameBootstrap] ❌ EvidenceService missing!");
            
            if (_clueService != null)
            {
                container.Register<IClueService>(_clueService);
                LogRegistration("ClueService");
            }
            else Debug.LogError("[GameBootstrap] ❌ ClueService missing!");
            
            if (_hackingService != null)
            {
                container.Register<IHackingService>(_hackingService);
                LogRegistration("HackingService");
            }
            else Debug.LogError("[GameBootstrap] ❌ HackingService missing!");
            
            // Settings
            if (_gameSettings != null)
            {
                container.Register(_gameSettings);
                LogRegistration("GameSettings");
            }
            else Debug.LogWarning("[GameBootstrap] ⚠️ GameSettings missing");
        }
        
        private void ConfigureApplication()
        {
            if (_gameSettings != null)
            {
                Application.targetFrameRate = _gameSettings.TargetFrameRate;
                QualitySettings.vSyncCount = _gameSettings.VSync ? 1 : 0;
                
                if (_audioService != null)
                {
                    _audioService.SetMasterVolume(_gameSettings.MasterVolume);
                    _audioService.SetMusicVolume(_gameSettings.MusicVolume);
                    _audioService.SetSFXVolume(_gameSettings.SFXVolume);
                }
            }
        }
        
        private void Start()
        {
            LoadInitialScene();
        }
        
        private async void LoadInitialScene()
        {
            if (_sceneService != null)
            {
                await _sceneService.LoadSceneAsync("MainMenu", showLoadingScreen: false);
            }
        }
        
        private void LogRegistration(string serviceName)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[GameBootstrap] ✅ Registered: {serviceName}");
            }
        }
        
        private void OnApplicationQuit()
        {
            Dependencies.Container.Clear();
        }
        
#if UNITY_EDITOR
        [ContextMenu("Test Save System")]
        private async void TestSaveSystem()
        {
            if (_saveService == null)
            {
                Debug.LogError("SaveService not initialized!");
                return;
            }
            
            Debug.Log("==================== SAVE SYSTEM TEST ====================");
            
            var testData = new Data.SaveData
            {
                Player = new Data.PlayerData 
                { 
                    PlayerName = "TestDetective",
                    Level = 5,
                    CasesCompleted = 3
                },
                CurrentCaseId = "case_001",
                TotalPlayTime = 1234.5f
            };
            
            Debug.Log("Testing save...");
            bool saveSuccess = await _saveService.SaveToSlotAsync(0, testData);
            Debug.Log($"Save: {(saveSuccess ? "✅ SUCCESS" : "❌ FAILED")}");
            
            Debug.Log("Testing load...");
            var loadedData = await _saveService.LoadFromSlotAsync(0);
            
            if (loadedData != null)
            {
                Debug.Log("✅ Load SUCCESS");
                Debug.Log($"  Player: {loadedData.Player.PlayerName}");
                Debug.Log($"  Level: {loadedData.Player.Level}");
                Debug.Log($"  Cases: {loadedData.Player.CasesCompleted}");
            }
            else
            {
                Debug.LogError("❌ Load FAILED");
            }
            
            var slots = _saveService.GetAllSaveSlots();
            Debug.Log($"Found {slots.Count} save slots");
            
            foreach (var slot in slots)
            {
                Debug.Log($"  Slot {slot.SlotIndex}: {slot.LastSaveTime} - Cloud: {slot.IsCloudSynced}");
            }
            
            Debug.Log("========================================================");
        }
        
        [ContextMenu("Show Save Info")]
        private void ShowSaveInfo()
        {
            if (_saveService == null)
            {
                Debug.LogError("SaveService not initialized!");
                return;
            }
            
            Debug.Log("==================== SAVE SERVICE INFO ====================");
            Debug.Log($"Platform: {_saveService.CurrentPlatform}");
            Debug.Log($"Cloud Enabled: {_saveService.IsCloudEnabled}");
            Debug.Log($"Service Type: {_saveService.GetType().Name}");
            Debug.Log("=========================================================");
        }
        
        [ContextMenu("Validate All Services")]
        private void ValidateAllServices()
        {
            Debug.Log("==================== SERVICE VALIDATION ====================");
            
            bool allValid = true;
            
            allValid &= ValidateService(_eventService, "EventService");
            allValid &= ValidateService(_saveService as MonoBehaviour, "SaveService");
            allValid &= ValidateService(_audioService, "AudioService");
            allValid &= ValidateService(_sceneService, "SceneService");
            allValid &= ValidateService(_inputService, "InputService");
            allValid &= ValidateService(_windowManager, "WindowManager");
            allValid &= ValidateService(_evidenceService, "EvidenceService");
            allValid &= ValidateService(_clueService, "ClueService");
            allValid &= ValidateService(_hackingService, "HackingService");
            allValid &= ValidateService(_gameSettings, "GameSettings");
            
            Debug.Log("==========================================================");
            
            if (allValid)
            {
                Debug.Log("✅ ALL SERVICES VALIDATED!");
            }
            else
            {
                Debug.LogError("❌ SOME SERVICES MISSING!");
            }
        }
        
        private bool ValidateService(Object service, string serviceName)
        {
            bool isValid = service != null;
            Debug.Log($"  {(isValid ? "✅" : "❌")} {serviceName}");
            return isValid;
        }
#endif
    }
}