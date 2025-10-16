using Unity.VisualScripting;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UI;
public class BaseballBatItem : MonoBehaviour, IInventoryItem, IInteractable
{
    [SerializeField] private GameObject _heldVisual;
    [SerializeField] private BaseballBatItemSO _baseballBatSO;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Collider _collider;
    private GameObject _owner;
    private bool _hasOwner => _owner != null;
    private bool _isInOwnerHand = false;
    private GameObject _currentHeldVisual;
    private Timer _attackCooldownTimer = new Timer(1);
    private bool _canAttack = true;
    private float attackTime = 2f;
    private void Update()
    {
        if (_hasOwner)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        _attackCooldownTimer.TimerUpdate(Time.deltaTime);
        if (_attackCooldownTimer.IsComplete)
        {
            _canAttack = true;
        }
    }
    void PerformMeleeAttack()
    {
        LayerMask enemyLayer = LayerMask.GetMask("Enemy");

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward
            * _baseballBatSO.AttackDistance * 0.5f, _baseballBatSO.AttackRadius, enemyLayer);
        if (hitEnemies.Length > 0)
        {
            //play hit sound??
            //Debug.Log("?A?DA?");
            AudioManager.Instance.PlayByKey3D("BaseBallBatHit", hitEnemies[0].transform.position);
        }

        foreach (Collider enemy in hitEnemies)
        {
            enemy.gameObject.GetComponent<IHitable>()?.OnHit(_owner,
                _baseballBatSO.Damage, _baseballBatSO.KnockoutPower);
            // Debug.Log(enemy.gameObject.name);
            //  enemy.GetComponent<EnemyHealth>().TakeDamage(attackDamage);
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
        if (_canAttack)
        {
            PerformMeleeAttack();
            _attackCooldownTimer.Reset(attackTime);
            _canAttack = false;
        }
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
        return _baseballBatSO.ItemName;
    }
    public bool IsPocketSize()
    {
        return _baseballBatSO.IsPocketSize;
    }
    public GameObject GetHeldVisual()
    {
        return _heldVisual;
    }
    public Image GetUIImage()
    {
        return _baseballBatSO.ItemUIImage;
    }

    //change to raw value struct
    public ScienceData GetValueStruct()
    {
        return new ScienceData { rawScienceValue = 0 };
    }

}
