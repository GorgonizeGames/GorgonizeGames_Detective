using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Services;

namespace Game.Runtime.UI.Windows
{
    public class WindowManager : MonoBehaviour, IWindowManager
    {
        [Header("Settings")]
        [SerializeField] private int baseZOrder = 100;
        [SerializeField] private int zOrderIncrement = 10;
        
        [Inject] private IEventService _eventService;
        [Inject] private IAudioService _audioService;
        
        private readonly Dictionary<string, IWindow> _registeredWindows = new Dictionary<string, IWindow>();
        private readonly List<IWindow> _openWindows = new List<IWindow>();
        private int _currentMaxZOrder;
        
        private void Start()
        {
            Dependencies.Inject(this);
            _currentMaxZOrder = baseZOrder;
            
            AutoRegisterWindows();
        }
        
        private void AutoRegisterWindows()
        {
            WindowBase[] windows = FindObjectsOfType<WindowBase>(true);
            foreach (var window in windows)
            {
                RegisterWindow(window);
            }
        }
        
        public void RegisterWindow(IWindow window)
        {
            if (window == null) return;
            
            if (!_registeredWindows.ContainsKey(window.WindowId))
            {
                _registeredWindows[window.WindowId] = window;
            }
        }
        
        public void UnregisterWindow(IWindow window)
        {
            if (window != null && _registeredWindows.ContainsKey(window.WindowId))
            {
                _registeredWindows.Remove(window.WindowId);
                _openWindows.Remove(window);
            }
        }
        
        public void OpenWindow(string windowId)
        {
            if (!_registeredWindows.TryGetValue(windowId, out IWindow window))
            {
                Debug.LogWarning($"[WindowManager] Window '{windowId}' not found");
                return;
            }
            
            if (window.IsOpen)
            {
                BringToFront(window);
                return;
            }
            
            window.ZOrder = GetNextZOrder();
            window.Open();
            
            if (!_openWindows.Contains(window))
            {
                _openWindows.Add(window);
            }
        }
        
        public void OpenWindow<T>() where T : WindowBase
        {
            string windowId = typeof(T).Name;
            OpenWindow(windowId);
        }
        
        public void CloseWindow(string windowId)
        {
            if (_registeredWindows.TryGetValue(windowId, out IWindow window))
            {
                window.Close();
                _openWindows.Remove(window);
            }
        }
        
        public void CloseWindow<T>() where T : WindowBase
        {
            string windowId = typeof(T).Name;
            CloseWindow(windowId);
        }
        
        public void CloseAllWindows()
        {
            var windowsToClose = _openWindows.ToList();
            foreach (var window in windowsToClose)
            {
                window.Close();
            }
            _openWindows.Clear();
        }
        
        public void BringToFront(IWindow window)
        {
            if (window == null || !window.IsOpen) return;
            
            window.ZOrder = GetNextZOrder();
            window.Focus();
            
            foreach (var openWindow in _openWindows)
            {
                if (openWindow != window && openWindow.IsFocused)
                {
                    openWindow.Blur();
                }
            }
            
            _openWindows.Sort((a, b) => a.ZOrder.CompareTo(b.ZOrder));
        }
        
        public void SendToBack(IWindow window)
        {
            if (window == null || !window.IsOpen) return;
            
            window.ZOrder = baseZOrder;
            window.Blur();
        }
        
        public bool IsWindowOpen(string windowId)
        {
            return _registeredWindows.TryGetValue(windowId, out IWindow window) && window.IsOpen;
        }
        
        public bool IsWindowOpen<T>() where T : WindowBase
        {
            string windowId = typeof(T).Name;
            return IsWindowOpen(windowId);
        }
        
        public T GetWindow<T>() where T : WindowBase
        {
            string windowId = typeof(T).Name;
            if (_registeredWindows.TryGetValue(windowId, out IWindow window))
            {
                return window as T;
            }
            return null;
        }
        
        public IWindow GetWindow(string windowId)
        {
            _registeredWindows.TryGetValue(windowId, out IWindow window);
            return window;
        }
        
        public List<IWindow> GetOpenWindows()
        {
            return new List<IWindow>(_openWindows);
        }
        
        private int GetNextZOrder()
        {
            _currentMaxZOrder += zOrderIncrement;
            return _currentMaxZOrder;
        }
    }
}