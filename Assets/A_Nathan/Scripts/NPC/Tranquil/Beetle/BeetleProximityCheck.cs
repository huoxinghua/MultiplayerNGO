using UnityEngine;

public class BeetleProximityCheck : MonoBehaviour
{
    [SerializeField] BeetleLineOfSight beetleLineOfSight;
    [SerializeField] SphereCollider sphereCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sphereCollider.radius = beetleLineOfSight.viewDistance;
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 6)
        {
            beetleLineOfSight.AddPlayerInProximity(other.gameObject);
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 6)
        {
            beetleLineOfSight.RemovePlayerFromProximity(other.gameObject);
        }
    }
}
