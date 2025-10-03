using UnityEngine;

public class BrutePiece : MonoBehaviour, ITwoHandItem,IInteractable
{
    [SerializeField] GameObject _heldView;
    [SerializeField] Renderer _renderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
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
    // Update is called once per frame
    void Update()
    {
        
    }
}
