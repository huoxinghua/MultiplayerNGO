using UnityEngine;

public class FFlashLight : MonoBehaviour ,IInteractable
{



    //Obsolete for now


    //Trash. Replaced. For now







    [SerializeField] GameObject heldVersionPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void OnInteract(GameObject interactingPlayer)
    {
        Debug.Log("Grabbed");
        if (interactingPlayer.GetComponent<Inventory>() != null)
        {
            Debug.Log("Grabbed");
            if (interactingPlayer.GetComponent<Inventory>().PickUpItem(heldVersionPrefab))
            {
                Destroy(gameObject);
            }
           
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
