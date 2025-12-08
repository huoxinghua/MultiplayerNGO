using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem.SampleItem
{
    [CreateAssetMenu(fileName = "SampleSO", menuName = "Sample/SampleSO")]
    public class SampleSO : ScriptableObject
    {
        [field: SerializeField] public SampleType SampleType { get; private set; }

        public float GetRandomTranquilValue()
        {
            float value = Random.Range(0f, 1f);
            return value;  
        }


        public float GetRandomViolentValue()
        {
            float value = Random.Range(0f, 1f);
            return value;
        }
        public float GetRandomMiscValue()
        {
            float value = Random.Range(0f, 1f);
            return value;
        }
    }
}
