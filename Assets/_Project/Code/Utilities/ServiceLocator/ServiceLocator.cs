using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Utilities.ServiceLocator
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, IService> _services = new();
        private static readonly HashSet<Type> _initializing = new();

        public static T Register<T>(T service) where T : IService
        {
            var type = typeof(T);
            
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"Service {type.Name} is already registered. Returning existing service.");
                return (T)_services[type];
            }

            _services[type] = service;
            
            if (!_initializing.Contains(type))
            {
                _initializing.Add(type);
                service.Initialize();
                _initializing.Remove(type);
            }
            
            return service;
        }

        public static T Get<T>() where T : IService
        {
            var type = typeof(T);
            
            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }

            Debug.LogError($"Service {type.Name} not found. Make sure it's registered.");
            return default;
        }

        public static bool TryGet<T>(out T service) where T : IService
        {
            var type = typeof(T);
            
            if (_services.TryGetValue(type, out var foundService))
            {
                service = (T)foundService;
                return true;
            }

            service = default;
            return false;
        }

        public static void Unregister<T>() where T : IService
        {
            var type = typeof(T);
            
            if (_services.TryGetValue(type, out var service))
            {
                service.Dispose();
                _services.Remove(type);
            }
        }

        public static void Clear()
        {
            foreach (var service in _services.Values)
            {
                service.Dispose();
            }
            
            _services.Clear();
            _initializing.Clear();
        }
    }
}