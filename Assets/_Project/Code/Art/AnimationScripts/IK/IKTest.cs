using UnityEngine;

namespace _Project.Code.Art.AnimationScripts.IK
{
    public class IKTest_NoNetwork : MonoBehaviour
    {
        [Header("Assign IK Controllers")]
        [SerializeField] private PlayerIKController fpsController;
        [SerializeField] private PlayerIKController tpsController;

        [Header("IK Interactable")]
        [SerializeField] private IKInteractable ik;

        [Header("Test Settings")]
        [SerializeField] private float walkSpeed = 1f;
        [SerializeField] private bool testAsFPS = true;

        private PlayerIKController CurrentController => testAsFPS ? fpsController : tpsController;

        void Update()
        {
            /*if (Input.GetKeyDown(KeyCode.F))
            {
                testAsFPS = !testAsFPS;
                testAsFPS = !testAsFPS;
                Debug.Log("testAsFPS: " + testAsFPS);
            }
            // PICKUP ANIMATION
            if (Input.GetKeyDown(KeyCode.K))
            {
                ik.PickupAnimation(CurrentController, testAsFPS);
                Debug.Log($"[TEST] Pickup ({(testAsFPS ? "FPS" : "TPS")})");
            }

            // IK IDLE
            if (Input.GetKeyDown(KeyCode.L))
            {
                ik.IKAnim.PlayIKIdle(testAsFPS);
            }

            // IK WALK
            if (Input.GetKeyDown(KeyCode.V))
            {
                ik.IKAnim.PlayIKMove(1, testAsFPS, false);
            }

            // IK RUN
            if (Input.GetKeyDown(KeyCode.B))
            {
                ik.IKAnim.PlayIKMove(1, testAsFPS, true);
            }

            // IK INTERACT
            if (Input.GetKeyDown(KeyCode.N))
            {
                ik.IKAnim.PlayIKInteract(testAsFPS);
            }

            // DROP
            if (Input.GetKeyDown(KeyCode.M))
            {
                ik.DropAnimation();
            }*/
        }
    }
}