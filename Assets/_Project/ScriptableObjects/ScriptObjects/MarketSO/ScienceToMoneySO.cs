using UnityEngine;

namespace _Project.ScriptableObjects.ScriptObjects.MarketSO
{
    [CreateAssetMenu(fileName = "ScienceToMoneySO", menuName = "Market/ScienceToMoneySO")]
    public class ScienceToMoneySO : ScriptableObject
    {
        [field: SerializeField] public float TranquilMoneyModifier { get; private set; }
        [field: SerializeField] public float ViolentMoneyModifier { get; private set; }
        [field: SerializeField] public float MiscMoneyModifier { get; private set; }
    }
}
