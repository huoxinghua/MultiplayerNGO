using _Project.Code.Art.AnimationScripts.IK;
using UnityEngine;

public class IKTest : MonoBehaviour
{
    [SerializeField] private IKInteractable ik;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ik.PlayIKIdle();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            ik.PlayIKWalk();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            ik.PlayIKRun();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            ik.PlayIKInteract();
        }
            
    }
}
