using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Game.Runtime.Core.Data;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Events;

namespace Game.Runtime.Core.Services
{
    public class LocalSaveService : MonoBehaviour, ISaveService
    {
        [Header("Settings")]
        [SerializeField] private string _saveFolder = "Saves";
        [SerializeField] private string _fileExtension = ".json";
        
        [Inject] private IEventService _eventService;
        
        private string _savePath;
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
        
        public SavePlatform CurrentPlatform => SavePlatform.Local;
        public bool IsCloudEnabled => false;
        
        public event Action<string> OnSaveStarted;
        public event Action<string, bool> OnSaveCompleted;
        public event Action<string> OnLoadStarted;
        public event Action<string, bool> OnLoadCompleted;
        public event Action OnCloudSyncStarted;
        public event Action<bool> OnCloudSyncCompleted;
        
        private void Awake()
        {
            _savePath = Path.Combine(Application.persistentDataPath, _saveFolder);
            
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }
        }
        
        private void Start()
        {
            Dependencies.Inject(this);
        }
        
        public async Task<bool> SaveDataAsync<T>(string key, T data) where T : class
        {
            if (string.IsNullOrEmpty(key) || data == null) return false;
            
            OnSaveStarted?.Invoke(key);
            
            try
            {
                string json = JsonUtility.ToJson(data, true);
                string filePath = GetFilePath(key);
                
                await File.WriteAllTextAsync(filePath, json);
                
                _cache[key] = data;
                OnSaveCompleted?.Invoke(key, true);
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalSaveService] Save failed for '{key}': {e.Message}");
                OnSaveCompleted?.Invoke(key, false);
                return false;
            }
        }
        
        public async Task<T> LoadDataAsync<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key)) return null;
            
            OnLoadStarted?.Invoke(key);
            
            try
            {
                if (_cache.TryGetValue(key, out object cached))
                {
                    OnLoadCompleted?.Invoke(key, true);
                    return cached as T;
                }
                
                string filePath = GetFilePath(key);
                
                if (!File.Exists(filePath))
                {
                    OnLoadCompleted?.Invoke(key, false);
                    return null;
                }
                
                string json = await File.ReadAllTextAsync(filePath);
                T data = JsonUtility.FromJson<T>(json);
                
                _cache[key] = data;
                OnLoadCompleted?.Invoke(key, true);
                
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalSaveService] Load failed for '{key}': {e.Message}");
                OnLoadCompleted?.Invoke(key, false);
                return null;
            }
        }
        
        public async Task<bool> DeleteDataAsync(string key)
        {
            try
            {
                string filePath = GetFilePath(key);
                
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                }
                
                _cache.Remove(key);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalSaveService] Delete failed for '{key}': {e.Message}");
                return false;
            }
        }
        
        public async Task<bool> DeleteAllDataAsync()
        {
            try
            {
                if (Directory.Exists(_savePath))
                {
                    await Task.Run(() => Directory.Delete(_savePath, true));
                    Directory.CreateDirectory(_savePath);
                }
                
                _cache.Clear();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalSaveService] Delete all failed: {e.Message}");
                return false;
            }
        }
        
        public async Task<bool> SaveToSlotAsync(int slotIndex, SaveData data)
        {
            string key = $"save_slot_{slotIndex}";
            return await SaveDataAsync(key, data);
        }
        
        public async Task<SaveData> LoadFromSlotAsync(int slotIndex)
        {
            string key = $"save_slot_{slotIndex}";
            return await LoadDataAsync<SaveData>(key);
        }
        
        public async Task<bool> DeleteSlotAsync(int slotIndex)
        {
            string key = $"save_slot_{slotIndex}";
            return await DeleteDataAsync(key);
        }
        
        public List<SaveSlotInfo> GetAllSaveSlots()
        {
            var slots = new List<SaveSlotInfo>();
            
            if (!Directory.Exists(_savePath)) return slots;
            
            var files = Directory.GetFiles(_savePath, $"save_slot_*{_fileExtension}");
            
            foreach (var file in files)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string indexStr = fileName.Replace("save_slot_", "");
                    
                    if (int.TryParse(indexStr, out int index))
                    {
                        var fileInfo = new FileInfo(file);
                        
                        slots.Add(new SaveSlotInfo
                        {
                            SlotIndex = index,
                            SlotName = $"Save Slot {index + 1}",
                            LastSaveTime = fileInfo.LastWriteTime,
                            FileSizeBytes = fileInfo.Length,
                            IsCloudSynced = false
                        });
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LocalSaveService] Error reading slot info: {e.Message}");
                }
            }
            
            return slots;
        }
        
        public bool HasLocalSave(string key) => File.Exists(GetFilePath(key));
        public bool HasCloudSave(string key) => false;
        
        public DateTime? GetLastSaveTime(string key)
        {
            string filePath = GetFilePath(key);
            return File.Exists(filePath) ? File.GetLastWriteTime(filePath) : null;
        }
        
        public long GetSaveFileSize(string key)
        {
            string filePath = GetFilePath(key);
            return File.Exists(filePath) ? new FileInfo(filePath).Length : 0;
        }
        
        public Task<bool> SyncWithCloudAsync() => Task.FromResult(false);
        public Task<bool> UploadToCloudAsync() => Task.FromResult(false);
        public Task<bool> DownloadFromCloudAsync() => Task.FromResult(false);
        
        private string GetFilePath(string key) => Path.Combine(_savePath, key + _fileExtension);
    }
}
