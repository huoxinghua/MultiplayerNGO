using UnityEngine;
using System.Collections;

public class CeilingLight : MonoBehaviour
{
    public Material _oldMaterial;
    public Material _newMaterial;
    private Renderer _objRenderer;
    private Light _spotLightSource;
    private Light _pointLightSource;
    private bool _isFlickering;
    public float _minIntensity = 0.1f;
    public float _maxIntensity = 2f;
    public float _originalIntensity = 0.6f;

    void Start()
    {
        _objRenderer = GetComponent<Renderer>();
        _spotLightSource = transform.GetChild(0)?.GetComponent<Light>();
        _pointLightSource = transform.GetChild(1)?.GetComponent<Light>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TurnLightOn();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            TurnLightOff();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartFlickering();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            StopFlickering();
        }
    }

    private void TurnLightOn()
    {
        _pointLightSource.enabled = true;
        _spotLightSource.enabled = true;
        _objRenderer.material = _oldMaterial;
    }

    private void TurnLightOff()
    {
        _pointLightSource.enabled = false;
        _spotLightSource.enabled = false;
        _objRenderer.material = _newMaterial;
    }

    private void StartFlickering()
    {
        _isFlickering = true;
        StartCoroutine(LightFlicker());
    }
    private void StopFlickering()
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
