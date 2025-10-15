using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FlashlightItem : MonoBehaviour, IInventoryItem, IInteractable
{
    [SerializeField] private GameObject _heldVisual;
    [SerializeField] private FlashItemSO _flashSO;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Light _sceneLight;
    [SerializeField] private Collider _collider;
    private float _currentCharge;
    [SerializeField]private bool _isFlashOn = false;
    [SerializeField] private bool _lastFlashState = true;
    private GameObject _owner;
    private bool _hasOwner => _owner != null;
    private bool _isInOwnerHand = false;
    private bool _hasCharge => _currentCharge >= 0;

    private Light _lightComponent;
    private GameObject _currentHeldVisual;
    private void Awake()
    {
        _sceneLight.enabled = false;
        _currentCharge = _flashSO.MaxCharge;
    }
    private void Update()
    {
        if (_isFlashOn) _currentCharge -= _flashSO.ChargeLoseRate * Time.deltaTime;
        if (_currentCharge <= 0) _isFlashOn = false;
        if (_currentHeldVisual == null) return;
        if(_hasOwner)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        if (_currentHeldVisual.activeInHierarchy && _lastFlashState != _isFlashOn)
        {
            _lightComponent.enabled = _isFlashOn;
            _lastFlashState = _isFlashOn;
        }
    }
    public void OnInteract(GameObject interactingPlayer)
    {
        if (interactingPlayer.GetComponent<PlayerInventory>().TryPickupItem())
        {
            interactingPlayer.GetComponent<PlayerInventory>().DoPickup(this);
        }
    }
    public void PickupItem(GameObject player, Transform playerHoldPosition)
    {
        _owner = player;
        _rb.isKinematic = true;
        _renderer.enabled = false;
        _collider.enabled = false;

        transform.parent = playerHoldPosition;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0,0,0);
        _currentHeldVisual = Instantiate(_heldVisual, playerHoldPosition);
        _lightComponent = _currentHeldVisual.GetComponent<Light>();
        _lightComponent.enabled = _isFlashOn;
        Debug.Log(_currentHeldVisual?.name);
        _sceneLight.enabled = false;
    }
    public void DropItem(Transform dropPoint)
    {
        _owner = null;

        _renderer.enabled = true;

        Destroy(_currentHeldVisual);
        _sceneLight.enabled = _isFlashOn;
        _lightComponent = null;
        transform.parent = null;

        _rb.isKinematic = false;
        _collider.enabled = true;
        transform.position = dropPoint.position;
    }
    private void ToggleFlashLight()
    {
        if (!_hasCharge)
        {
            _isFlashOn = false;
            return;
        }
        _isFlashOn = !_isFlashOn;
    }
    public void UseItem()
    {
        if (!_isInOwnerHand) return;
        ToggleFlashLight();
    }
    public void UnequipItem()
    {
        _currentHeldVisual?.SetActive(false);
        _lightComponent.enabled = false;
        _isInOwnerHand = false;
    }
    public void EquipItem()
    {
        _currentHeldVisual?.SetActive(true);
        _lightComponent.enabled = _isFlashOn;
        _isInOwnerHand = true;
    }
    public string GetItemName()
    {
        return _flashSO.ItemName;
    }
    public bool IsPocketSize()
    {
        return _flashSO.IsPocketSize;
    }
    public GameObject GetHeldVisual()
    {
        return _heldVisual;
    }
    public Image GetUIImage()
    {
        return _flashSO.ItemUIImage;
    }
    public int GetSampleMonValue()
    {
        return 0;
    }
    public float GetSampleSciValue()
    {
        return 0;
    }
}
