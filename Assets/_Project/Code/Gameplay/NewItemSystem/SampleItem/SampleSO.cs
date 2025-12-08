using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem.SampleItem
{
    [CreateAssetMenu(fileName = "SampleSO", menuName = "Sample/SampleSO")]
    public class SampleSO : ScriptableObject
    {
        [field: SerializeField] public SampleType SampleType { get; private set; }

        public int GetRandomTranquilValue()
        {
            float value = Random.Range(0f, 1f);
            return Mathf.RoundToInt(value);  
        }


        public int GetRandomViolentValue()
        {
            float value = Random.Range(0f, 1f);
            return Mathf.RoundToInt(value);
        }
        public int GetRandomMiscValue()
        {
            float value = Random.Range(0f, 1f);
            return Mathf.RoundToInt(value);
        }
    }
}
