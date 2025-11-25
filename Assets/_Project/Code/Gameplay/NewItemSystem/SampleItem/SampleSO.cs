using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem.SampleItem
{
    [CreateAssetMenu(fileName = "SampleSO", menuName = "Sample/SampleSO")]
    public class SampleSO : ScriptableObject
    {
        [field: SerializeField] public string SampleType { get; private set; }
        
        [Header("Tranquil Range")]
        [field: Range(0,1), SerializeField ] public float MinTranquilRange { get; private set; }
        [field:Range(0,1),  SerializeField] public float MaxTranquilRange { get; private set; }

        [Header("Violent Range")] 
        [field: Range(0,1), SerializeField] public float MinViolentRange { get; private set; }
        [field: Range(0,1), SerializeField] public float MaxViolentRange { get; private set; }
        [Header("Misc Range")]
        [field: Range(0,1), SerializeField] public float MinMiscRange { get; private set; }
        [field: Range(0,1), SerializeField] public float MaxMiscRange { get; private set; }
   
        public int GetRandomTranquilValue()
        {
            float value = Random.Range(MinTranquilRange, MaxTranquilRange);
            return Mathf.RoundToInt(value * 100);  
        }


        public int GetRandomViolentValue()
        {
            float value = Random.Range(MinViolentRange, MaxViolentRange);
            return Mathf.RoundToInt(value * 100);
        }
        public int GetRandomMiscValue()
        {
            float value = Random.Range(MinMiscRange, MaxMiscRange);
            return Mathf.RoundToInt(value * 100);
        }
    }
}
