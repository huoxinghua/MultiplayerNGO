using UnityEngine;

public class SampleJarView : MonoBehaviour, IView
{

    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider col;
    [SerializeField] private GameObject heldVisual;
    public GameObject currentVisual;
    public void DestroyHeldVisual()
    {
        if (currentVisual == null) return;
        Destroy(currentVisual);
    }

    public void DisplayHeld(Transform position)
    {
        if (currentVisual != null) return;
        currentVisual = Instantiate(heldVisual, position);
        currentVisual.transform.parent = position;
    }

    public GameObject GetCurrentVisual()
    {
        return currentVisual;
    }

    public void MoveToPosition(Vector3 position)
    {
        transform.position = position;
        transform.parent = null;
    }

    public void SetLightEnabled(bool on)
    {
        throw new System.NotImplementedException();
    }

    public void SetPhysicsEnabled(bool enabled)
    {
        rb.isKinematic = !enabled;
        col.enabled = enabled;
        if (!enabled)
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

    public void SetVisible(bool visible)
    {
        meshRenderer.enabled = visible;
    }
}
