using UnityEngine;

[CreateAssetMenu(fileName = "SampleSO", menuName = "Sample/SampleSO")]
public class SampleSO : ScriptableObject
{
    public string sampleType;

    [Header("Research Value Range")]
    public float minResearchValue = 50f;
    public float maxResearchValue = 75f;

    [Header("Money Value Range")]
    public int minMoneyValue = 20;
    public int maxMoneyValue = 50;

   
    public float GetRandomResearchValue()
    {
        return Random.Range(minResearchValue, maxResearchValue);
    }


    public int GetRandomMoneyValue()
    {
        return Random.Range(minMoneyValue, maxMoneyValue);
    }
}
