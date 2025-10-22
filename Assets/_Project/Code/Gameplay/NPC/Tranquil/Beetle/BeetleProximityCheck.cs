using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Tranquil.Beetle
{
    public class BeetleProximityCheck : MonoBehaviour
    {
        [SerializeField] private BeetleLineOfSight _beetleLineOfSight;
        [SerializeField] private SphereCollider _sphereCollider;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _sphereCollider.radius = _beetleLineOfSight.viewDistance;
        }

        public void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.layer == 6)
            {
                _beetleLineOfSight.AddPlayerInProximity(other.gameObject);
            }
        }
        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == 6)
            {
                _beetleLineOfSight.RemovePlayerFromProximity(other.gameObject);
            }
        }
    }
}
