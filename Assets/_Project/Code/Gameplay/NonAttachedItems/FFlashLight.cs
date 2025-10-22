using _Project.Code.Gameplay.Interactables;
using _Project.Code.Gameplay.Player;
using UnityEngine;

namespace _Project.Code.Gameplay.NonAttachedItems
{
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
}
