using UnityEngine;

namespace _Project.Code.Gameplay.Interactables
{
    public static class BetterInterface
    {
        public static T GetInterface<T>(this GameObject obj) where T : class
        {
            foreach (var mono in obj.GetComponents<MonoBehaviour>())
            {
                if (mono is T iface)
                    return iface;
            }
            return null;
        }
    }
}

