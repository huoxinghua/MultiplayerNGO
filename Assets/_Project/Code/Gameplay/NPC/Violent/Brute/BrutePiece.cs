using UnityEngine;
using UnityEngine.UI;

public class BrutePiece : MonoBehaviour, IInventoryItem ,IInteractable
{
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Collider _collider;
    [SerializeField] private GameObject _heldVisual;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private BruteItemSO _brutePieceSO;
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
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(0, 0, 0);
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
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        _currentHeldVisual = Instantiate(_heldVisual, playerHoldPosition);
    }
    public void DropItem(Transform dropPoint)
    {
        _owner = null;
        _renderer.enabled = true;
        Destroy(_currentHeldVisual);
        transform.parent = null;
        _rb.isKinematic = false;
        _collider.enabled = true;
        transform.position = dropPoint.position;
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
        return _brutePieceSO.ItemName;
    }
    public bool IsPocketSize()
    {
        return _brutePieceSO.IsPocketSize;
    }
    public GameObject GetHeldVisual()
    {
        return _heldVisual;
    }
    public Image GetUIImage()
    {
        return _brutePieceSO.ItemUIImage;
    }

    //change to raw value struct
    public ScienceData GetValueStruct()
    {
        return new ScienceData { RawTranquilValue = _tranquilValue, RawMiscValue = _miscValue, RawViolentValue = _violentValue };
    }

    public void OnInteract(GameObject interactingPlayer)
    {
        var inventory = interactingPlayer.GetComponent<PlayerInventory>();
        if (inventory != null && inventory.TryPickupItem())
        {
            inventory.DoPickup(this);
        }
    }
    void OnEnable()
    {
        transform.parent = null;
    }
}
