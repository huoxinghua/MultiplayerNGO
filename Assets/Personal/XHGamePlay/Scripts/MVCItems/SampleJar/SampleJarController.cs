using UnityEngine;

public class SampleJarController : MonoBehaviour,IHeldItem, IInteractable
{
    private SampleJarModel model;
    private IView view;
    private void Awake()
    {
        model = new SampleJarModel();
        view = GetComponent<IView>();
    }
    public void Drop()
    {
        if (!model.HasOwner || !model.IsInHand) return;

        Transform dropPoint = model.Owner.transform.GetChild(3); 
        view.MoveToPosition(dropPoint.position);
        view.DestroyHeldVisual();
        view.SetVisible(true);
        view.SetPhysicsEnabled(true);
   

        model.ClearOwner();
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

    public void Pickup()
    {
        view.SetVisible(false);
        view.SetPhysicsEnabled(false);
        view.DisplayHeld(model.Owner.transform.GetChild(0).GetChild(0));
        transform.parent = model.Owner.transform.GetChild(0).GetChild(0);
        transform.localPosition = new Vector3(0, 0, 0);
        transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    public void SwapOff()
    {
        throw new System.NotImplementedException();
    }

    public void SwapTo()
    {
        if (!model.HasOwner && view.GetCurrentVisual() != null) return;
      
        view.DisplayHeld(model.Owner.transform.GetChild(0).GetChild(0));
        model.InHand(true);
    }

    public void Use()
    {
        throw new System.NotImplementedException();
    }

}
