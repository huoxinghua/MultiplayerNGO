using UnityEngine;

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
    [SerializeField] GameObject pressEText;
    [SerializeField] GameObject holdEText;
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
            Debug.DrawRay(cameraTransform.position, transform.forward * interactDist, Color.red);
            Debug.Log("Raycast hit: " + hit.transform.name);
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
            pressEText.SetActive(hasInteractable);
            holdEText.SetActive(!hasInteractable && hasHoldInteract);
        }
        else
        {
            // Raycast hit nothing, clear everything
            currentTarget = null;
            currentInteractable = null;
            currentHoldInteract = null;
            castedInteract = false;
            pressEText.SetActive(false);
            holdEText.SetActive(false);
        }
    }
    public void AttemptInteract()
    {
        RaycastHit hit;
        /*        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
                RaycastHit[] hits = Physics.RaycastAll(ray, interactDist, ~0, QueryTriggerInteraction.Collide);
                foreach (var h in hits)
                {
                    Debug.Log($"RaycastAll hit: {h.collider.name}, IsTrigger: {h.collider.isTrigger}");
                }
        */
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactDist, lM, QueryTriggerInteraction.Collide))
        {
            // Debug.Log($"RaycastAll hit: {hit.collider.name}, IsTrigger: {hit.collider.isTrigger}");
            if (hit.collider.transform.gameObject.GetComponent<IInteractable>() != null)
            {
                if (hit.collider.transform.gameObject.GetComponent<IInOutDoor>() != null)
                {
                    IInOutDoor temp = hit.collider.transform.gameObject.GetComponent<IInOutDoor>();
                    inOutTransform = temp.UseDoor();
                    timeToInteract = temp.GetTimeToOpen();
                    isHolding = true;
                    startInteractPos = playerObj.transform.position;
                }
                lastInteracted = hit.transform.gameObject;
                hit.collider.transform.gameObject.GetComponent<IInteractable>().OnInteract(playerObj);
            }
            if (hit.collider.transform.gameObject.GetComponent<IHoldToInteract>() != null)
            {
                Debug.Log("HitIshere>!<>!");
                currentHold = hit.collider.transform.gameObject.GetComponent<IHoldToInteract>();
                hit.collider.transform.gameObject.GetComponent<IHoldToInteract>().OnHold(playerObj);
            }
        }
    }
}
