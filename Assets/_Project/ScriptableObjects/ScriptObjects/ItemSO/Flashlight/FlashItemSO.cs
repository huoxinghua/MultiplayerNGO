using UnityEngine;

namespace _Project.ScriptableObjects.ScriptObjects.ItemSO.Flashlight
{
    [CreateAssetMenu(fileName = "FlashItemSO", menuName = "Items/FlashItemSO")]
    public class FlashItemSO : BaseItemSO
    {
        [field: Header("Flashlight Charge")]
        [field: SerializeField] public float MaxCharge;
        [field: SerializeField] public float ChargeLoseRate;
        [field: Header("Flashlight Light Data")]
        [field: SerializeField] public float InnerLightRadius { get; private set; }
        [field: SerializeField] public float OuterLightRadius { get; private set; }
        [field: SerializeField] public float LightIntensity { get; private set; }
        [field: SerializeField] public float LightRange { get; private set; }

    }

}
