using System;
using System.Reflection;
using UnityEngine;

namespace Game.Runtime.Core.DI
{
    public static class Dependencies
    {
        private static readonly object _lock = new object();
        private static DIContainer _container;
        
        public static DIContainer Container
        {
            get
            {
                if (_container == null)
                {
                    lock (_lock)
                    {
                        if (_container == null)
                        {
                            _container = new DIContainer();
                        }
                    }
                }
                return _container;
            }
        }
        
        public static void Inject(MonoBehaviour obj)
        {
            if (obj == null)
            {
                Debug.LogError("Cannot inject dependencies into null object");
                return;
            }
            
            try
            {
                var fields = obj.GetType().GetFields(
                    BindingFlags.NonPublic | 
                    BindingFlags.Instance);
                
                foreach (var field in fields)
                {
                    var injectAttr = field.GetCustomAttribute<InjectAttribute>();
                    if (injectAttr != null)
                    {
                        try
                        {
                            var resolveMethod = typeof(DIContainer).GetMethod("Resolve")?.MakeGenericMethod(field.FieldType);
                            if (resolveMethod != null)
                            {
                                var service = resolveMethod.Invoke(Container, null);
                                field.SetValue(obj, service);
                            }
                        }
                        catch (TargetInvocationException e)
                        {
                            var innerException = e.InnerException ?? e;
                            Debug.LogError($"Failed to inject dependency for field '{field.Name}' in '{obj.GetType().Name}': {innerException.Message}", obj);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Failed to inject dependency for field '{field.Name}' in '{obj.GetType().Name}': {e.Message}", obj);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to inject dependencies into '{obj.GetType().Name}': {e.Message}", obj);
            }
        }
        
        public static bool TryInject(MonoBehaviour obj)
        {
            try
            {
                Inject(obj);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class InjectAttribute : Attribute 
    {
        public bool Required { get; set; } = true;
        
        public InjectAttribute(bool required = true)
        {
            Required = required;
        }
    }
}