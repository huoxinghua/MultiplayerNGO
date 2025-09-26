using UnityEngine;

public class BaseballBatView : MonoBehaviour , IView
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider col;
    [SerializeField] private GameObject heldVisual;
    public GameObject currentVisual;
    public Animator _currentAnimator;
    public void SetLightEnabled(bool on)
    {
        //temporary NEEDS MAJOR REFACTOR
        if(_currentAnimator != null)
        _currentAnimator.SetTrigger("PlaySwing");
    }
    public GameObject GetCurrentVisual()
    {
        return currentVisual;
    }
    public void SetVisible(bool visible)
    {
        meshRenderer.enabled = visible;
    }
    public void DisplayHeld(Transform position)
    {
        if (currentVisual != null) return;
        currentVisual = Instantiate(heldVisual, position);
        currentVisual.transform.parent = position;
        _currentAnimator = currentVisual.GetComponentInChildren<Animator>();
    }
    public void DestroyHeldVisual()
    {
        if (currentVisual == null) return;
        _currentAnimator = null;
        Destroy(currentVisual);
        
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

    public void MoveToPosition(Vector3 position)
    {
        transform.position = position;
        transform.parent = null;
    }
}
