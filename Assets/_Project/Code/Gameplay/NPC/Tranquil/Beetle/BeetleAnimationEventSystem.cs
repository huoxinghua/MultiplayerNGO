using _Project.Code.Utilities.Audio;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Tranquil.Beetle
{
    public class BeetleAnimationEventSystem : MonoBehaviour
    {
        public void OnFootStep()
        {
            AudioManager.Instance.PlayByKey3D("BeetleFootStep", transform.position);
        }
    }
}
