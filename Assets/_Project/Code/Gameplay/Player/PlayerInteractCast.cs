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
                // Only update cache if the target changes
                if (hitRoot != currentTarget)
                {
                    currentTarget = hitRoot;
                    // Check for interfaces on hit object or parent (for nested colliders like PickUpCollider)
                    currentInteractable = currentTarget.GetComponent<IInteractable>()
                                          ?? currentTarget.GetComponentInParent<IInteractable>();
                    currentHoldInteract = currentTarget.GetComponent<IHoldInteract>()
                                          ?? currentTarget.GetComponentInParent<IHoldInteract>();
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
            RaycastHit hit;

            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactDist, lM, QueryTriggerInteraction.Collide))
            {
                // Check for interfaces on hit object or parent (for nested colliders)
                var interactable = hit.collider.GetComponent<IInteractable>()
                                   ?? hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    lastInteracted = hit.transform.gameObject;
                    interactable.OnInteract(playerObj);
                }
                var holdToInteract = hit.collider.GetComponent<IHoldToInteract>()
                                     ?? hit.collider.GetComponentInParent<IHoldToInteract>();
                if (holdToInteract != null)
                {
                    currentHold = holdToInteract;
                    holdToInteract.OnHold(playerObj);
                }
            }
        }
    }
}
