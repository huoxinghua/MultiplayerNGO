using UnityEngine;

namespace _Project.Code.Art.AnimationScripts.Animations
{
    public class DollTest : MonoBehaviour
    {
        [SerializeField] private DollAnimation dollAnim;
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                dollAnim.PlaySwitchPose();
                Debug.Log("Switch Pose");
            }
        }
    }
}
