using UnityEngine;

public class BeetleAnimationEventSystem : MonoBehaviour
{
    public void OnFootStep()
    {
        AudioManager.Instance.PlayByKey3D("BeetleFootStep", transform.position);
    }
}
