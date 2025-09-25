using UnityEngine;

public class AudioTester : MonoBehaviour
{

    void Start()
    {
        AudioManager.Instance.PlayByKey3D("SampleCollected", transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
