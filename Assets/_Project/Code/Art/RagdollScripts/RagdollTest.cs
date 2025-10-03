using UnityEngine;

public class RagdollTest : MonoBehaviour
{
    [SerializeField] Ragdoll ragdoll;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (ragdoll != null)
            {
                ragdoll.EnableRagdoll();
            }
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (ragdoll != null)
            {
                ragdoll.EnableAnimator();
            }
        }
    }
}
