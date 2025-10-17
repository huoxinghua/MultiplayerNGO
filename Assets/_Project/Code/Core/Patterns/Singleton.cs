using UnityEngine;
namespace _Project.Code.Core.Patterns
{
    // Stolen Valor
    /* public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
     {

         #region Properties

         private static T _instance; //all private fields for properties are denoted with an underscore in front of the name
         public static T Instance
         {
             get
             {
                 _instance = FindAnyObjectByType<T>();
                 // If the instance is not already set, create a new instance
                 if (_instance == null)
                 {
                     *//*var singletonObject = new GameObject(typeof(T).Name);
                 _instance = singletonObject.AddComponent<T>();*//*
                     Debug.LogWarning($"No instance of {typeof(T).Name} found in the scene.");
                 }

                 // Return the instance
                 return _instance;
             }
         }

         [field: SerializeField] public bool IsPersistent { get; private set; } // Set the instance to persist between scenes (don't destroy on load)

         #endregion

         #region Protected Methods  

         // Ensure the instance isn't destryoyed when loading a new scene
         private void Awake()
         {
             if (_instance == null)
             {
                 _instance = this as T;

                 // If the instance is set to persist between scenes, don't destroy it
                 if (IsPersistent)
                     DontDestroyOnLoad(gameObject);
             }
             else if (_instance != this)
             {
                 // If another instance already exists, destroy this one
                 Destroy(gameObject);
             }
         }

         // Clear the instance when the object is destroyed
         private void OnDestroy()
         {
             if (_instance == this)
             {
                 _instance = null;
             }
         }

         #endregion
     }*/
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _isApplicationQuitting;
        protected virtual bool PersistBetweenScenes => true;

        public static T Instance
        {
            get
            {
                if (_isApplicationQuitting) return null;

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();
                    if (_instance == null)
                    {
                        GameObject singletonObject = new();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T) + " (Singleton)";
                    }
                }

                return _instance;
            }
        }


        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (PersistBetweenScenes) DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
        }
    }
}