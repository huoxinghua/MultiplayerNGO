using UnityEngine;

public class SampleObjTest : MonoBehaviour,ISampleable
{
    [SerializeField] SampleSO sampleSO;

    public SampleSO GetSample()
    {
        return sampleSO;
    }

  
}
