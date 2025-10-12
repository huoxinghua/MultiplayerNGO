using UnityEngine;

public class BeetleDead : MonoBehaviour, IInteractable, ITwoHandItem
{
    [SerializeField] private GameObject _heldView;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Transform _beetleTransform;
    [SerializeField] private GameObject _beetleSkele;
    [SerializeField] private Collider _collider;

    public void OnEnable()
    {
        _collider.enabled = true; 
    }
    public void OnPickup()
    {
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
        var inventory = interactingPlayer.GetComponent<Inventory>();
        if (inventory != null && inventory.PickUpTwoHanded(_heldView, _beetleSkele))
        {
            OnPickup();
        }
    }
}
