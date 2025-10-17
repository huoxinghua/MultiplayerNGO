using UnityEngine;

public class SwingDoorEnemyCheck : MonoBehaviour
{
    [SerializeField] private SwingDoors _doorScript;
    public void OnTriggerEnter(Collider other)
    {
        if (!_doorScript.IsDoorOpen() && other.gameObject.layer == 7)
        {
            _doorScript.ToggleOpen();
        }
    }
}
