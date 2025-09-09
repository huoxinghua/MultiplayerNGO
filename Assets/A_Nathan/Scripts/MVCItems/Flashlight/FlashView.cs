using UnityEngine;

public class FlashView : MonoBehaviour , IView
{
    [SerializeField] private Light flashlightLight;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider col;

    public void SetLightEnabled(bool on)
    {
        flashlightLight.enabled = on;
    }

    public void SetVisible(bool visible)
    {
        meshRenderer.enabled = visible;
    }

    public void SetPhysicsEnabled(bool enabled)
    {
        rb.isKinematic = !enabled;
        col.enabled = enabled;
        if(!enabled)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.interpolation = RigidbodyInterpolation.None;
            rb.detectCollisions = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Extrapolate;
            rb.detectCollisions = true;
        }
    }

    public void MoveToPosition(Vector3 position)
    {
        transform.position = position;
        transform.parent = null;
    }
}
