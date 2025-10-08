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
            if (inventory.currentItem == this.gameObject)
            {
                SwapTo();
            }
            else
            {
                SwapOff();
            }
        }
    }

    public void Use()
    {
        if (!model.HasOwner || !model.IsInHand) return;
        AudioManager.Instance.PlayByKey3D("FlashLightClick", model.Owner.transform.position);
        model.Toggle();
        view.SetLightEnabled(model.IsOn);
    }
    public void SwapOff()
    {

       
        if (!model.HasOwner && view.GetCurrentVisual() == null) return;

        //probably a swap animation here?

        view.DestroyHeldVisual();
        model.InHand(false);
    }
    public void SwapTo()
    {
        if (!model.HasOwner && view.GetCurrentVisual() != null) return;
        //probably a swap animation here
        view.DisplayHeld(model.Owner.transform.GetChild(0).GetChild(0));
        model.InHand(true);
    }
    public void Drop()
    {
        if (!model.HasOwner || !model.IsInHand) return;

        Transform dropPoint = model.Owner.transform.GetChild(1); // or some drop reference
        view.MoveToPosition(dropPoint.position);
        view.DestroyHeldVisual();
        view.SetVisible(true);
        view.SetPhysicsEnabled(true);
     //   view.SetLightEnabled(false); // turn off when dropped. maybe. Might be funnier if they can stay on

        model.ClearOwner();
    }

    public void Pickup()
    {
        view.SetVisible(false);
        view.SetPhysicsEnabled(false);
        view.DisplayHeld(model.Owner.transform.GetChild(0).GetChild(0));
        transform.parent = model.Owner.transform.GetChild(0).GetChild(0);
        transform.localPosition = new Vector3(0,0,0);
        transform.localRotation = Quaternion.Euler(0,0,0);
    }
}
