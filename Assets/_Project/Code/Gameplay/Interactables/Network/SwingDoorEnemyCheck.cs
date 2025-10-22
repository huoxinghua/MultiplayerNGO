using UnityEngine;

namespace _Project.Code.Gameplay.Interactables.Network
{
    public class SwingDoorEnemyCheck : MonoBehaviour
    {
        [SerializeField] private SwingDoors _doorScript;
        public void OnTriggerEnter(Collider other)
        {
            if (!_doorScript.IsDoorOpen() && other.gameObject.layer == 7)
            {
                _doorScript.EnemyOpened();
            }
        }
    }
}
