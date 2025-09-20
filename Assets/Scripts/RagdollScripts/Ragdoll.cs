using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Ragdoll : MonoBehaviour
{
    [SerializeField] private Transform ragdollRoot;
    [SerializeField] private bool RagdollEneble = false;

    //private NavMeshAgent agent;
    private Rigidbody[] jointRBs;
    private Collider[] jointColls;
    private Animator animator;
    

    private void Awake()
    {
        //agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        animator.enabled = true;

        jointRBs = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        jointColls = ragdollRoot.GetComponentsInChildren<Collider>();
    }

    private void Start()
    {
        if (RagdollEneble) EnableRagdoll();
        else EnableAnimator();
    }

    public void EnableRagdoll()
    {
        animator.enabled = false;
        //agent.enabled = false;

        foreach(Collider joint in jointColls)
        {
            joint.enabled = true;
        }

        foreach(Rigidbody rb in jointRBs)
        {
            rb.detectCollisions = true;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
        }
    }

    public void DisableAllRigidbodies()
    {
        foreach(Rigidbody rb in jointRBs)
        {
            rb.detectCollisions = false;
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    public void EnableAnimator()
    {
        animator.enabled = true;
        //agent.enabled = true;

        foreach(Collider joint in jointColls)
        {
            joint.enabled = false;
        }

        foreach(Rigidbody rb in jointRBs)
        {
            rb.detectCollisions = false;
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }
}
