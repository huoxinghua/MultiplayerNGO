using System;
using UnityEngine;

namespace _Project.Code.Core.ServiceLocator
{
    public abstract class MonoBehaviourService : MonoBehaviour, IService
    {
        public event Action OnReady;
        public virtual void Initialize() { OnReady?.Invoke(); }

        public virtual void Dispose()
        {
            if (gameObject)
            {
                Destroy(gameObject);
            }
        }
    }
}