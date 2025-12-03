using UnityEngine;

namespace _Project.Code.Gameplay.Scripts.MVCItems.SampleJar
{
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
        
        /*[Header("Tranquil Range")]
        [field: Range(0,1), SerializeField ] public float MinTranquilRange { get; private set; }
        [field:Range(0,1),  SerializeField] public float MaxTranquilRange { get; private set; }

        [Header("Violent Range")] 
        [field: Range(0,1), SerializeField] public float MinViolentRange { get; private set; }
        [field: Range(0,1), SerializeField] public float MaxViolentRange { get; private set; }
        [Header("Misc Range")]
        [field: Range(0,1), SerializeField] public float MinMiscRange { get; private set; }
        [field: Range(0,1), SerializeField] public float MaxMiscRange { get; private set; }*/
   
        public int GetRandomResearchValue()
        {
            return Random.Range(minResearchValue, maxResearchValue);
        }


        public int GetRandomMoneyValue()
        {
            return Random.Range(minMoneyValue, maxMoneyValue);
        }
    }
}
