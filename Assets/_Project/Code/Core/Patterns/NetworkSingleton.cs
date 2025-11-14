using System.Collections;
using Unity.Netcode;
using UnityEngine;
namespace _Project.Code.Core.Patterns
{
    public abstract class NetworkSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
    {
        /// <summary>
        /// Global access point for the singleton instance.
        /// </summary>
        public static T Instance { get; private set; }

        /// <summary>
        /// Whether this singleton should persist between scene loads.
        /// Override to return false if you want it destroyed on scene change.
        /// </summary>
        protected virtual bool PersistBetweenScenes => true;
        protected virtual bool AutoSpawn => true;
        protected virtual void Awake()
        {
            // Check if instance already exists
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[{typeof(T).Name}] Multiple instances detected. Destroying duplicate on {gameObject.name}");
                Destroy(gameObject);
                return;
            }

            // Set instance
            Instance = this as T;

            // Persist between scenes if configured
            if (PersistBetweenScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            Debug.Log($"[{typeof(T).Name}] Singleton initialized");
            if (AutoSpawn)
                StartCoroutine(AutoNetworkSpawnRoutine());
        }

        private IEnumerator AutoNetworkSpawnRoutine()
        {
       
            yield return new WaitUntil(() => Unity.Netcode.NetworkManager.Singleton != null);
            var manager = Unity.Netcode.NetworkManager.Singleton;

       
            yield return new WaitUntil(() => manager.IsListening);

         
            yield return new WaitUntil(() => NetworkObject != null);
            yield return null; 

            if (manager.IsServer && !NetworkObject.IsSpawned)
            {
                NetworkObject.Spawn();
            }
        }


        public override void OnDestroy()
        {
            base.OnDestroy();

            // Clear instance reference if we're the active instance
            if (Instance == this)
            {
                Instance = null;
                Debug.Log($"[{typeof(T).Name}] Singleton destroyed");
            }
        }

        /// <summary>
        /// Called when the NetworkObject is spawned.
        /// Override this to add custom initialization logic.
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Debug.Log($"[{typeof(T).Name}] Network spawned. IsServer={IsServer}, IsClient={IsClient}");
        }

        /// <summary>
        /// Called when the NetworkObject is despawned.
        /// Override this to add custom cleanup logic.
        /// </summary>
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            Debug.Log($"[{typeof(T).Name}] Network despawned");
        }
    }
}