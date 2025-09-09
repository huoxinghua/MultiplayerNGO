using UnityEngine;

public class FlashCont : MonoBehaviour , IHeldItem , IInteractable
{
    private FlashModel model;
    private IView view;

    private void Awake()
    {
        model = new FlashModel();
        view = GetComponent<IView>();
    }
    public void Start()
    {
        view.SetLightEnabled(model.IsOn);
    }
    public void OnInteract(GameObject interactingPlayer)
    {
        var inventory = interactingPlayer.GetComponent<Inventory>();
        if (inventory != null && inventory.PickUpItem(gameObject))
        {
            model.SetOwner(interactingPlayer);
            Pickup();
        }
    }

    public void Use()
    {
        if (!model.HasOwner) return;

        model.Toggle();
        view.SetLightEnabled(model.IsOn);
        Debug.Log("Flashlight toggled: " + model.IsOn);
    }

    public void Drop()
    {
        if (!model.HasOwner) return;

        Transform dropPoint = model.Owner.transform.GetChild(4); // or some drop reference
        view.MoveToPosition(dropPoint.position);

        view.SetVisible(true);
        view.SetPhysicsEnabled(true);
        view.SetLightEnabled(false); // turn off when dropped

        model.ClearOwner();
    }

    public void Pickup()
    {
        view.SetVisible(false);
        view.SetPhysicsEnabled(false);
        transform.parent = model.Owner.transform.GetChild(3);
        transform.localPosition = new Vector3(0,0,0);
        transform.localRotation = Quaternion.Euler(0,0,0);
    }
}
