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
    public bool IsDoorOpen() {return _isOpen; }
    public void ToggleOpen()
    {
        transform.localRotation = Quaternion.Euler(0f, _isOpen ? 0f : 90f, 0f);
        _isOpen = !_isOpen;
        AudioManager.Instance.PlayByKey3D("DoorOpen", transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
