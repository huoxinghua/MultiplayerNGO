using UnityEngine;

namespace _Project.Code.Gameplay.Interactables
{
    public class MainExit : MonoBehaviour , IInOutDoor,IInteractable
    {
        [SerializeField] float timeToHold;
        bool isInteracting = false;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }
        public void OnInteract(GameObject interactingPlayer)
        { 
            //   interactingPlayer.transform.position = GameObject.Find("MainExitPoint").transform.position;
        }
        public Transform UseDoor()
        {
            return GameObject.Find("MainExitPoint").transform;
        }
        public float GetTimeToOpen()
        {
            return timeToHold;
        }
        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
