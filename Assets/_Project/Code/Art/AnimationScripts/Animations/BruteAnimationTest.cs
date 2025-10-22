using UnityEngine;

namespace _Project.Code.Art.AnimationScripts.Animations
{
    public class BruteAnimationTest : MonoBehaviour
    {
        [SerializeField] private BruteAnimation bruteAnim;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                bruteAnim.PlayNormal();
                Debug.Log("Switch to Normal (bruteStatus = Normal)");
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                bruteAnim.PlayAlert();
                Debug.Log("Switch to Alert (bruteStatus = Alert)");
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                bruteAnim.PlayInjured();
                Debug.Log("Switch to Injured (bruteStatus = Injured)");
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                bruteAnim.PlayWalk(10, 10);
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                bruteAnim.PlayRun(10, 10);
            }

            if(Input.GetKeyDown(KeyCode.A))
            {
                bruteAnim.PlayAttack();
            }
        }
    }
}
