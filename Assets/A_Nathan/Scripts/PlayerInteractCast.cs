using System.Xml.Serialization;
using UnityEngine;

public class PlayerInteractCast : MonoBehaviour
{
    [SerializeField] GameObject PlayerObj;
    [SerializeField]
    Transform cameraTransform;
    [SerializeField]
    float interactDist;
    [SerializeField] 
    LayerMask lM;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            AttemptInteract();
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, transform.forward, out hit, interactDist, lM))
        {
            if(hit.transform.gameObject.GetComponent<IInteractable>()!= null)
            {
                Debug.Log("IsInteractable");
            }
        }
    }
    public void AttemptInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, transform.forward, out hit, interactDist, lM))
        {
            if (hit.transform.gameObject.GetComponent<IInteractable>() != null)
            {
                hit.transform.gameObject.GetComponent<IInteractable>().OnInteract(PlayerObj);
            }
        }
    }
}
