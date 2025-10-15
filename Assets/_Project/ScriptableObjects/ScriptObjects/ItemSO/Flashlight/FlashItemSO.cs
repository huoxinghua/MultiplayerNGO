using UnityEngine;

[CreateAssetMenu(fileName = "FlashItemSO", menuName = "Scriptable Objects/FlashItemSO")]
public class FlashItemSO : BaseItemSO
{
    [field: Header("Flashlight Charge")]
    [field: SerializeField] public float MaxCharge;
    [field: SerializeField] public float ChargeLoseRate;
}
