using UnityEngine;

namespace _Project.Code.Core.ServiceLocator
{
    public abstract class MonoBehaviourService : MonoBehaviour, IService
    {
        public virtual void Initialize() { }

        public virtual void Dispose()
        {
            if (gameObject)
            {
                Destroy(gameObject);
            }
        }
    }
}