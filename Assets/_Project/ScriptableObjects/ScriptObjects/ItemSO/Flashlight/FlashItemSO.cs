using UnityEngine;

namespace _Project.ScriptableObjects.ScriptObjects.ItemSO.Flashlight
{
    [CreateAssetMenu(fileName = "FlashItemSO", menuName = "Items/FlashItemSO")]
    public class FlashItemSO : BaseItemSO
    {
        [field: Header("Flashlight Charge")]
        [field: SerializeField] public float MaxCharge;
        [field: SerializeField] public float ChargeLoseRate;
    }
}
