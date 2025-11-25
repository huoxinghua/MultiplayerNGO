using UnityEngine;

[CreateAssetMenu(fileName = "QuotaDataSO", menuName = "Market/QuotaDataSO")]
public class QuotaDataSO : ScriptableObject
{
    //To determine start values
    [field: Header("Quota At Start Of Playthrough")]
    [field: SerializeField] private float MinStartQuota;
    [field: SerializeField] private float MaxStartQuota;
    public float RandomStartQuota => Random.Range(MinStartQuota, MaxStartQuota);
    
    //To determine increase to quota after a successful quota
    [field: Header("Quota Increase On Success")]
    [field: SerializeField] private float MinIncreaseQuota;
    [field: SerializeField] private float MaxIncreaseQuota;
    public float RandomIncreaseQuota => Random.Range(MinIncreaseQuota, MaxIncreaseQuota);
    
    //The amount of days before a quota is to be checked
    [field: Header("Days In A Quota")]
    [field: SerializeField] public float DaysInAQuota { get; private set; }
}
