using UnityEngine;

public class BruteAnimationEventController : MonoBehaviour
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
        AudioManager.Instance.PlayByKey3D("BruteFootStep", transform.position);
    }
    public void OnAttackNoise()
    {
        AudioManager.Instance.PlayByKey3D("BruteAttack", transform.position);
    }
    public void OnAttackConnect()
    {

    }
    public void OnAttackEnd()
    {

    }
}
