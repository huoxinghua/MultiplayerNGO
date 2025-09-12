using System.Xml.Serialization;
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
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, transform.forward, out hit, interactDist, lM))
        {
            if(hit.transform.gameObject.GetComponent<IInteractable>()!= null)
            {
                castedInteract = true;
                //Indicate to player that obj is interactable    
            }
            else
            {
                castedInteract = false;
            }
        }

        if (isHolding)
        {
            if (!castedInteract || Vector3.Distance(playerObj.transform.position,startInteractPos) > releaseDistance)
            {
                ReleaseInteract();
            }
            timeInteracted += Time.deltaTime;
            if (timeInteracted > timeToInteract&&inOutTransform != null)
            {
                
                playerObj.transform.position = inOutTransform.position;
                playerObj.transform.rotation = Quaternion.Euler(0,inOutTransform.rotation.eulerAngles.y,0);
                ReleaseInteract();
            }
        }
    }
    public void AttemptInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactDist, lM))
        {
            if (hit.transform.gameObject.GetComponent<IInteractable>() != null)
            {
                if(hit.transform.gameObject.GetComponent<IInOutDoor>() != null)
                {
                    IInOutDoor temp = hit.transform.gameObject.GetComponent<IInOutDoor>();
                    inOutTransform = temp.UseDoor();
                    timeToInteract = temp.GetTimeToOpen();
                    isHolding = true;
                    startInteractPos = playerObj.transform.position;
                }
                lastInteracted = hit.transform.gameObject;
                hit.transform.gameObject.GetComponent<IInteractable>().OnInteract(playerObj);
            }
        }
    }
}
