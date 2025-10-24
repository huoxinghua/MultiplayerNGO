using System.Collections;
using UnityEngine;

namespace _Project.Code.Gameplay.Scripts.LightFunction
{
    public class LightController : MonoBehaviour
    {
        private Light _spotLightSource;
        private Light _pointLightSource;
        private bool _isFlickering;
        public float _minIntensity = 0.1f;
        public float _maxIntensity = 0.6f;
        public float _originalIntensity = 0.4f;

        void Start()
        {
            _spotLightSource = transform.GetChild(0)?.GetComponent<Light>();
            _pointLightSource = transform.GetChild(1)?.GetComponent<Light>();
        }

        public void TurnLightOn()
        {
            _pointLightSource.enabled = true;
            _spotLightSource.enabled = true;
        }

        public void TurnLightOff()
        {
            _pointLightSource.enabled = false;
            _spotLightSource.enabled = false;
        }

        public void StartFlickering()
        {
            _isFlickering = true;
            StartCoroutine(LightFlicker());
        }
        public void StopFlickering()
        {
            _isFlickering = false;
            StopCoroutine(LightFlicker());
            _spotLightSource.intensity = _originalIntensity;
            _pointLightSource.intensity = _originalIntensity;
        }


        IEnumerator LightFlicker()
        {
            while (_isFlickering == true)
            {
                float randomIntensity = Random.RandomRange(_minIntensity, _maxIntensity);
                _spotLightSource.intensity = randomIntensity;
                _pointLightSource.intensity = randomIntensity;
                yield return new WaitForSeconds(0.1f);
            }
        }

    }
}
