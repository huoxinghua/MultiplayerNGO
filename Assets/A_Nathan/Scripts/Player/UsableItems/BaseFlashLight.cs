using UnityEngine;

public class BaseFlashLight : MonoBehaviour , IHeldItem , IInteractable
{
    public GameObject playerWithItem;
    


    ///Implement a charge system. Ideas to show to stand out. display a bar or battery life somehow --- Light flickers as the battery gets lower without much indication of the percent --- No charge --- combo of the first 2?
//when toggled on, must inform the held visual version to shine. Or even better, have the object this is attached to do the lighting, and when dropped, still shine. This will add to immersion and fun moments   
    public void OnInteract(GameObject interactingPlayer)
    {
        
        if (interactingPlayer.GetComponent<Inventory>() != null)
        {
          
            if (interactingPlayer.GetComponent<Inventory>().PickUpItem(gameObject))
            {
                playerWithItem = interactingPlayer;
                Pickup();
               
            }

        }
    }
    public void Use()
    {
        if (playerWithItem == null) return;
        
            Debug.Log("ToggleFlashLight");
    }
    public void Drop()
    {
        
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.GetComponent<Collider>().enabled = true;
        transform.position = playerWithItem.transform.GetChild(4).position;
        playerWithItem = null;
    }
    public void Pickup()
    {
       
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<Collider>().enabled = false;
    }
}
