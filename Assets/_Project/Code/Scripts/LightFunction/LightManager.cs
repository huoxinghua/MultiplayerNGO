using UnityEngine;

public class CeilingLight : MonoBehaviour
{
    public Material _oldMaterial;
    public Material _newMaterial;
    private Renderer _objRenderer;
    private Light _spotLightSource;
    private Light _pointLightSource;
    
    void Start()
    {
        _objRenderer = GetComponent<Renderer>();
        _spotLightSource = transform.GetChild(0)?.GetComponent<Light>();
        _pointLightSource = transform.GetChild(1)?.GetComponent<Light>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _objRenderer.material = _newMaterial;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _pointLightSource.enabled = !_pointLightSource.enabled;
            _spotLightSource.enabled = _pointLightSource.enabled;
        }
    }
}
