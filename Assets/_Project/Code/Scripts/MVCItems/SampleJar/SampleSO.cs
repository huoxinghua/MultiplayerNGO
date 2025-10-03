using UnityEngine;

[CreateAssetMenu(fileName = "SampleSO", menuName = "Sample/SampleSO")]
public class SampleSO : ScriptableObject
{
    [field: SerializeField] public string SampleType { get; private set; }

    [Header("Research Value Range")]
    public int minResearchValue = 50;
    public int maxResearchValue = 75;

    [Header("Money Value Range")]
    public int minMoneyValue = 20;
    public int maxMoneyValue = 50;

   
    public int GetRandomResearchValue()
    {
        return Random.Range(minResearchValue, maxResearchValue);
    }


    public int GetRandomMoneyValue()
    {
        return Random.Range(minMoneyValue, maxMoneyValue);
    }
}
