using UnityEngine;

public class BrutePiece : MonoBehaviour, ITwoHandItem,IInteractable
{
    [SerializeField] private GameObject _heldView;
    [SerializeField] private Renderer _renderer;
    void OnEnable()
    {
        transform.parent = null;
    }
    public void OnPickup()
    {
        _renderer.enabled = false;
    }
    public void OnDrop()
    {
        _renderer.enabled = true;
    }
    public void OnInteract(GameObject interactingPlayer)
    {
        var inventory = interactingPlayer.GetComponent<Inventory>();
        if (inventory != null && inventory.PickUpTwoHanded(_heldView, gameObject))
        {
            OnPickup();
        }
    }
}
