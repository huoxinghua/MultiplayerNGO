using UnityEngine;
using UnityEngine.UI;

public class BeetleDead : MonoBehaviour, IInteractable, IInventoryItem
{
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Transform _beetleTransform;
    [SerializeField] private GameObject _beetleSkele;
    [SerializeField] private Collider _collider;
    [SerializeField] private GameObject _heldVisual;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private BeetleItemSO _beetleSO;
    private GameObject _owner = null;
    private bool _hasOwner => _owner != null;
    private bool _isInOwnerHand = false;
    private GameObject _currentHeldVisual;

    private float _tranquilValue;
    private float _violentValue;
    private float _miscValue;

    public void Awake()
    {
        _tranquilValue = Random.Range(0f, 1f);
        _violentValue = Random.Range(0f, 1f);
        _miscValue = Random.Range(0f, 1f);
    }
    private void Update()
    {
        if (_hasOwner)
        {
            _beetleSkele.transform.localPosition = Vector3.zero;
            _beetleSkele.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }
    public void PickupItem(GameObject player, Transform playerHoldPosition)
    {
        _owner = player;
        _rb.isKinematic = true;
        _renderer.enabled = false;
        _collider.enabled = false;
        _beetleSkele.transform.parent = playerHoldPosition;
        _beetleSkele.transform.localPosition = Vector3.zero;
        _beetleSkele.transform.localRotation = Quaternion.Euler(0, 0, 0);
        _currentHeldVisual = Instantiate(_heldVisual, playerHoldPosition);
    }
    public void DropItem(Transform dropPoint)
    {
        _owner = null;
        _renderer.enabled = true;
        Destroy(_currentHeldVisual);
        _beetleSkele.transform.parent = null;
        _rb.isKinematic = false;
        _collider.enabled = true;
        _beetleSkele.transform.position = dropPoint.position;
    }
    public void UseItem()
    {

    }
    public void UnequipItem()
    {
        _currentHeldVisual?.SetActive(false);
        _isInOwnerHand = false;
    }
    public void EquipItem()
    {
        _currentHeldVisual?.SetActive(true);
        _isInOwnerHand = true;
    }
    public string GetItemName()
    {
        return _beetleSO.ItemName;
    }
    public bool IsPocketSize()
    {
        return _beetleSO.IsPocketSize;
    }
    public GameObject GetHeldVisual()
    {
        return _heldVisual;
    }
    public Image GetUIImage()
    {
        return _beetleSO.ItemUIImage;
    }
    public bool CanPickUp()
    {
        return _beetleSO.CanBeSold;
    }
    public bool CanBeSold()
    {
        return _beetleSO.CanBeSold;
    }
    public void WasSold()
    {
        Destroy(_currentHeldVisual);
        Destroy(_beetleSkele);
    }
    //change to raw value struct
    public ScienceData GetValueStruct()
    {
        return new ScienceData { RawTranquilValue = _tranquilValue, RawMiscValue = _miscValue, RawViolentValue = _violentValue, KeyName = _beetleSO.ItemName};
    }
    public void OnEnable()
    {
        _collider.enabled = true; 
    }
   
    public void OnInteract(GameObject interactingPlayer)
    {
        var inventory = interactingPlayer.GetComponent<PlayerInventory>();
        if (inventory != null && inventory.TryPickupItem())
        {
            inventory.DoPickup(this);
        }
    }
}
