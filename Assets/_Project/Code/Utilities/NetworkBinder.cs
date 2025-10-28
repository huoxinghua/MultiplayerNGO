using System;
using Unity.Netcode;

namespace _Project.Code.Utilities
{
    public static class NetworkBinder
    {
        /// <summary>
        /// Binds a MonoBehaviour property or field to a NetworkVariable.
        /// Keeps them in sync based on the specified direction.
        /// </summary>
        public static void Bind<T>(
            Func<T> getLocalValue,
            Action<T> setLocalValue,
            NetworkVariable<T> networkVariable,
            SyncDirection direction = SyncDirection.Both,
            NetworkBehaviour owner = null)
        {
            // ---- Network → Local ----
            if (direction == SyncDirection.NetworkToMono || direction == SyncDirection.Both)
            {
                networkVariable.OnValueChanged += (oldVal, newVal) =>
                {
                    setLocalValue?.Invoke(newVal);
                };

                // Initialize from network on spawn
                if (owner != null && owner.IsSpawned)
                {
                    setLocalValue?.Invoke(networkVariable.Value);
                }
            }

            // ---- Local → Network ----
            if (direction == SyncDirection.MonoToNetwork || direction == SyncDirection.Both)
            {
                // Optional: Hook this into your update loop, or use manual "Push" calls
                void PushToNetwork()
                {
                    if (owner != null && owner.IsServer)
                    {
                        var localVal = getLocalValue();
                        if (!Equals(localVal, networkVariable.Value))
                            networkVariable.Value = localVal;
                    }
                }

                // Simple pattern: update periodically
                if (owner != null)
                    owner.StartCoroutine(PollEveryFrame(PushToNetwork));
            }
        }

        private static System.Collections.IEnumerator PollEveryFrame(Action update)
        {
            while (true)
            {
                update?.Invoke();
                yield return null;
            }
        }
    
    }
    public enum SyncDirection
    {
        MonoToNetwork, // e.g., server sets local -> update NetworkVariable
        NetworkToMono, // e.g., sync client view from NetworkVariable
        Both            // e.g., continuous mirror
    }
}