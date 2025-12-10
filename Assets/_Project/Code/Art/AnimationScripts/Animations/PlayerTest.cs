using UnityEngine;

namespace _Project.Code.Art.AnimationScripts.Animations
{
    public class PlayerTest : MonoBehaviour
    {
        [SerializeField] private PlayerAnimation anim;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                anim.PlayWalk(0, 10);
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                anim.PlayWalk(10, 10);
            }

            if(Input.GetKeyDown(KeyCode.J))
            {
                anim.PlayRun(10, 10);
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                anim.PlayCrouch();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                anim.PlayStanding();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                anim.PlayJump();
            }
        }
    }
}
