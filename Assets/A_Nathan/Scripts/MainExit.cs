using UnityEngine;

public class MainExit : MonoBehaviour , IInteractable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void OnInteract(GameObject interactingPlayer)
    {
        interactingPlayer.transform.position = GameObject.Find("MainExitPoint").transform.position;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
