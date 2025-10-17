using Unity.Netcode.Components;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] Animator fpsAnim;
    [SerializeField] Animator tpsAnim;
    [SerializeField] NetworkAnimator networkAnim;

    public void SetMovement(float speed,bool isRunning)
    {
        fpsAnim.SetFloat("speed", speed);
        tpsAnim.SetFloat("speed", speed);

        tpsAnim.SetBool("isRunning", isRunning);
        tpsAnim.SetBool("isRunning", isRunning);

        //networkAnim
    }
}
