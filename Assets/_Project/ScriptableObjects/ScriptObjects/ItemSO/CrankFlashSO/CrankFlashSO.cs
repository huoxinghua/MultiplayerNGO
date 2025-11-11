using UnityEngine;

namespace _Project.ScriptableObjects.ScriptObjects.ItemSO.CrankFlashSO
{
    [CreateAssetMenu(fileName = "CrankFlashItemSO", menuName = "Items/CrankFlashItemSO")]
    public class CrankFlashSO : BaseItemSO
    {
        [field: Header("Flashlight Charge")]
        [field: SerializeField] public float MaxCharge;
        [field: SerializeField] public float ChargeLoseRate;
        [field: SerializeField] public float ChargeGainRate { get; private set; }

        [field: Header ("Flashlight Sound")]
        [field: SerializeField] public float SoundRange {  get; private set; }

        [field: Header("Flashlight Light Data")]
        [field: SerializeField] public float InnerLightRadius { get; private set; }
        [field: SerializeField] public float OuterLightRadius { get; private set; }
        [field: SerializeField] public float LightIntensity { get; private set; }
        [field: SerializeField] public float LightRange { get; private set; }
    }
}
