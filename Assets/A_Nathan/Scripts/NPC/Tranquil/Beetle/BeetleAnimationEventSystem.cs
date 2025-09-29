using UnityEngine;

public class BeetleAnimationEventSystem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnFootStep()
    {
        AudioManager.Instance.PlayByKey3D("BeetleFootStep", transform.position);
    }
}
