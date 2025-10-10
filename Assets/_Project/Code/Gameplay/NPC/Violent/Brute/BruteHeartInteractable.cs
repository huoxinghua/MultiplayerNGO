using UnityEngine;

public class BruteHeartInteractable : MonoBehaviour, ITwoHandItem, IInteractable
{
    [SerializeField] private GameObject _heldView;
    [SerializeField] private Renderer _renderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
