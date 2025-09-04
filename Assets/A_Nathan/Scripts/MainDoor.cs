using UnityEngine;

public class MainDoor : MonoBehaviour , IInteractable
{
    
    public void OnInteract(GameObject interactingPlayer)
    {
        interactingPlayer.transform.position = GameObject.Find("MainEntryPoint").transform.position;
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
