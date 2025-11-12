using _Project.ScriptableObjects.ScriptObjects.ItemSO;
using UnityEngine;

namespace _Project.ScriptableObjects.ScriptObjects.ItemSO.SyringeItem
{
    [CreateAssetMenu(fileName = "SyringeItemSO", menuName = "Items/SyringeItemSO")]
    public class SyringeItemSO : BaseItemSO
    {
        [field: Header("Syringe Data")]
        [field: SerializeField] public float SpeedBoostAmount { get; private set; }
        [field: SerializeField] public float EffectDuration { get; private set; }

    }
}
