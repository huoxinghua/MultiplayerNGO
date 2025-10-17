using UnityEngine;

public class SwingDoors : MonoBehaviour , IInteractable
{
    private bool _isOpen = false;
    private bool _openedByEnemy = false;
    private Timer _enemyOpenedTimer = new Timer(0);
    [SerializeField] private float _enemyCloseDelay;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void OnInteract(GameObject interactingPlayer)
    {
        ToggleOpen();
    }
    public bool IsDoorOpen() {return _isOpen; }
    public void EnemyOpened()
    {
        if (_isOpen) return;
        ToggleOpen();
        _enemyOpenedTimer.Reset(_enemyCloseDelay);
        _openedByEnemy=true;
    }
    public void ToggleOpen()
    {
        transform.localRotation = Quaternion.Euler(0f, _isOpen ? 0f : 90f, 0f);
        _isOpen = !_isOpen;
        AudioManager.Instance.PlayByKey3D("DoorOpen", transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        _enemyOpenedTimer?.TimerUpdate(Time.deltaTime);
        if(_enemyOpenedTimer.IsComplete && _openedByEnemy && _isOpen)
        {
            ToggleOpen();
            _openedByEnemy = false;
        }
    }

}
