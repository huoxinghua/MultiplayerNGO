using System.Collections.Generic;
using _Project.Code.Utilities.ServiceLocator;
using UnityEngine;

namespace _Project.Code.Core.Patterns
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("Persistent Services")]
        [SerializeField] private List<MonoBehaviourService> _servicePrefabs = new();

        private static bool _isInitialized = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad()
        {
            Application.quitting += ServiceLocator.Clear;
        }

        private void Awake()
        {
            if (_isInitialized)
            {
                Destroy(gameObject);
                return;
            }

            _isInitialized = true;
            DontDestroyOnLoad(gameObject);

            InitializeServices();
        }

        private void InitializeServices()
        {

            foreach (var servicePrefab in _servicePrefabs)
            {
                if (servicePrefab == null)
                {
                    Debug.LogWarning("[GameInitializer] Null service prefab in list");
                    continue;
                }

                var serviceInstance = Instantiate(servicePrefab, transform);
                var serviceType = serviceInstance.GetType();


                // Use reflection to call ServiceLocator.Register<T>(instance)
                var registerMethod = typeof(ServiceLocator)
                    .GetMethod(nameof(ServiceLocator.Register))
                    .MakeGenericMethod(serviceType);

                registerMethod.Invoke(null, new object[] { serviceInstance });

            }

        }
    }
}