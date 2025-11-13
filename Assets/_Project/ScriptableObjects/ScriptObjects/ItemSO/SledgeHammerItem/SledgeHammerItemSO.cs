using UnityEngine;

namespace _Project.ScriptableObjects.ScriptObjects.ItemSO.SledgeHammerItem
{
    [CreateAssetMenu(fileName = "SledgeHammerItemSO", menuName = "Items/SledgeHammerItemSO")]
    public class SledgeHammerItemSO : BaseItemSO
    {
        [field: Header("SledgeHammer Attack")]
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField] public float KnockoutPower { get; private set; }
        [field: SerializeField] public float AttackRadius { get; private set; }
    }
}

