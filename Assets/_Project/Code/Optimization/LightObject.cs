using _Project.Code.Utilities.Singletons;
using UnityEngine;

namespace _Project.Code.Optimization
{
    public class LightObject : MonoBehaviour
    {
        public Light LightComponent;
        private CurrentLights _currentLights;
        void Awake()
        {
            _currentLights = CurrentLights.Instance;
            _currentLights.AddLight(this);
            if (LightComponent == null)
                LightComponent = GetComponentInChildren<Light>();
        }

        public void SetActive(bool active)
        {
            LightComponent.enabled = active;
        }
    }
}
