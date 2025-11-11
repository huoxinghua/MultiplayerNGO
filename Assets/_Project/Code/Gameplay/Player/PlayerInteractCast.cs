using _Project.Code.Gameplay.Interactables;
using UnityEngine;

namespace _Project.Code.Gameplay.Player
{
    public class PlayerInteractCast : MonoBehaviour
    {
        //Note - Currently instantly interacts. I want to make the door open by holding down, but not for all interactables


        [SerializeField] GameObject playerObj;
        [SerializeField]
        Transform cameraTransform;
        [SerializeField]
        float interactDist;
        [SerializeField]
        LayerMask lM;
        GameObject lastInteracted;
        Transform inOutTransform;
        bool isHolding = false;
        float timeToInteract;
        float timeInteracted;
        bool castedInteract;
        Vector3 startInteractPos;
        [SerializeField] float releaseDistance;
        [SerializeField] IHoldToInteract currentHold;
        //set up interact!!!

        // PlayerInputManager inputManager;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }
        public void Update()
        {

            //Temp for now. Needs input action


            if (Input.GetKeyDown(KeyCode.E))
            {
                AttemptInteract();
            }
            if (Input.GetKeyUp(KeyCode.E))
            {
                ReleaseInteract();
            }

        }

        public void ReleaseInteract()
        {
            // Debug.Log("Release");
            isHolding = false;
            inOutTransform = null;
            timeToInteract = 0;
            timeInteracted = 0;
            currentHold?.OnRelease(playerObj);
            currentHold = null;
        }
        // Update is called once per frame
        private GameObject currentTarget = null;
        private IInteractable currentInteractable = null;
        private IHoldInteract currentHoldInteract = null;

        void FixedUpdate()
        {
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactDist, lM, QueryTriggerInteraction.Collide))
            {
                GameObject hitRoot = hit.collider.transform.gameObject;
                //  Debug.DrawRay(cameraTransform.position, transform.forward * interactDist, Color.red);
                //Debug.Log("Raycast hit: " + hit.transform.name);
                // Only update cache if the target changes
                if (hitRoot != currentTarget)
                {
                    currentTarget = hitRoot;
                    currentInteractable = currentTarget.GetComponent<IInteractable>();
                    currentHoldInteract = currentTarget.GetComponent<IHoldInteract>();
                }

                bool hasInteractable = currentInteractable != null;
                bool hasHoldInteract = currentHoldInteract != null;

                // UI Logic
                castedInteract = hasInteractable;
                if (hasInteractable)
                {
                    currentInteractable.HandleHover(true);
                }
            }
            else
            {
                // Raycast hit nothing, clear everything
                if(currentInteractable != null)
                {
                
                    currentInteractable.HandleHover(false);
                }
                if(currentHold != null)
                {
                    currentHold.OnRelease(playerObj);
                }
                currentTarget = null;
                currentInteractable = null;
                currentHoldInteract = null;
                castedInteract = false;
                currentHold = null;
            }
        }
        public void AttemptInteract()
        {
                Debug.Log("[playerinteractCast]Try to hit");
            RaycastHit hit;

            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactDist, lM, QueryTriggerInteraction.Collide))
            {
                Debug.Log("AttemptInteract:" + hit.collider.gameObject.name);
                if (hit.collider.transform.gameObject.GetComponent<IInteractable>() != null)
                {
                    lastInteracted = hit.transform.gameObject;
                    hit.collider.transform.gameObject.GetComponent<IInteractable>().OnInteract(playerObj);
                }
                if (hit.collider.transform.gameObject.GetComponent<IHoldToInteract>() != null)
                {
                    Debug.Log("WTF");
                    currentHold = hit.collider.transform.gameObject.GetComponent<IHoldToInteract>();
                    hit.collider.transform.gameObject.GetComponent<IHoldToInteract>().OnHold(playerObj);
                }
            }
        }
    }
}
