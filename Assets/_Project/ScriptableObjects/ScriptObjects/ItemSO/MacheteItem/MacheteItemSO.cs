using _Project.ScriptableObjects.ScriptObjects.ItemSO;
using UnityEngine;


namespace _Project.ScriptableObjects.ScriptObjects.ItemSO.MacheteItem
{
    [CreateAssetMenu(fileName = "MacheteItemSO", menuName = "Items/MacheteItemSO")]
    public class MacheteItemSO : BaseItemSO
    {
        [field: Header("Machete Attack")]
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField] public float KnockoutPower { get; private set; }
        [field: SerializeField] public float AttackRadius { get; private set; }

    }
}