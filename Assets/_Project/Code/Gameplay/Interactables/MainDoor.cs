using UnityEngine;

namespace _Project.Code.Gameplay.Interactables
{
    public class MainDoor : MonoBehaviour , IInteractable, IInOutDoor
    {
        [SerializeField] float timeToHold;
        bool isInteracting = false;
        public void OnInteract(GameObject interactingPlayer)
        {
            //interactingPlayer.transform.position = GameObject.Find("MainEntryPoint").transform.position;
        }
        public Transform UseDoor()
        {
            Debug.Log("Door");
            return GameObject.Find("MainEntryPoint").transform;
        }
        public float GetTimeToOpen()
        {
            return timeToHold;
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
