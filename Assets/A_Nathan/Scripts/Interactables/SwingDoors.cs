using UnityEngine;

public class SwingDoors : MonoBehaviour , IInteractable
{
    bool _isOpen = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void OnInteract(GameObject interactingPlayer)
    {
        ToggleOpen();
    }
    public void ToggleOpen()
    {
        if(!_isOpen)
        {
            transform.Rotate(0f, 90f, 0f,Space.Self);
        }
        else
        {
            transform.Rotate(0f, -90f, 0f,Space.Self);
        }
        _isOpen = !_isOpen;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
