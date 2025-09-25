using UnityEngine;

public class BeetleDead : MonoBehaviour, IInteractable, ITwoHandItem
{
    [SerializeField] GameObject _heldView;
 //   [SerializeField] BeetleState _beetleState;
    [SerializeField] Renderer _renderer;
    [SerializeField] Transform _beetleTransform;
    [SerializeField] GameObject _beetleSkele;
    [SerializeField] Collider _collider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void OnEnable()
    {
        _collider.enabled = true; 
    }
    public void OnPickup()
    {
        _beetleTransform.position = new Vector3(1000, -1000, 1000);
        _renderer.enabled = false;
        //hide ragdoll, disable collision with ragdoll/corspse, move to location that wont affect player
    }
    public void OnDrop()
    {
        _renderer.enabled = true;
        //show ragdoll, enable collsion
    }
    public void OnInteract(GameObject interactingPlayer)
    {
       // if(!_beetleState.IsEnemyDead() && !_beetleState.IsEnemyKnockedout()) return;
        var inventory = interactingPlayer.GetComponent<Inventory>();
        if (inventory != null && inventory.PickUpTwoHanded(_heldView, _beetleSkele))
        {
            OnPickup();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
