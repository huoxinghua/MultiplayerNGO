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
                LightComponent = GetComponent<Light>();
        }

        public void SetActive(bool active)
        {
            /*if(active == false)
            {
                Debug.Log("Deactive");
            }
            else
            {
                Debug.Log("Active");
            }*/
            LightComponent.enabled = active;
        }
    }
}
