using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.Core.Services
{
    public class EventService : MonoBehaviour, IEventService
    {
        private readonly Dictionary<Type, Delegate> _eventCallbacks = new Dictionary<Type, Delegate>();
        
        public void Subscribe<T>(Action<T> callback) where T : struct, IGameEvent
        {
            Type eventType = typeof(T);
            
            if (_eventCallbacks.ContainsKey(eventType))
            {
                _eventCallbacks[eventType] = Delegate.Combine(_eventCallbacks[eventType], callback);
            }
            else
            {
                _eventCallbacks[eventType] = callback;
            }
        }
        
        public void Unsubscribe<T>(Action<T> callback) where T : struct, IGameEvent
        {
            Type eventType = typeof(T);
            
            if (_eventCallbacks.ContainsKey(eventType))
            {
                _eventCallbacks[eventType] = Delegate.Remove(_eventCallbacks[eventType], callback);
                
                if (_eventCallbacks[eventType] == null)
                {
                    _eventCallbacks.Remove(eventType);
                }
            }
        }
        
        public void Publish<T>(T eventData) where T : struct, IGameEvent
        {
            Type eventType = typeof(T);
            
            if (_eventCallbacks.TryGetValue(eventType, out Delegate callback))
            {
                try
                {
                    (callback as Action<T>)?.Invoke(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventService] Error publishing {eventType.Name}: {e.Message}");
                }
            }
        }
        
        public void Clear()
        {
            _eventCallbacks.Clear();
        }
        
        private void OnDestroy()
        {
            Clear();
        }
    }
}
