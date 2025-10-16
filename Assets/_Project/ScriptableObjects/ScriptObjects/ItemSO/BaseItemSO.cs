using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "BaseItemSO", menuName = "Items/BaseItemSO")]
public class BaseItemSO : ScriptableObject
{
    [field: Header("Base Item Details")]
    [field: SerializeField] public string ItemName { get; private set; }
    [field: SerializeField] public Image ItemUIImage { get; private set; }
    [field: SerializeField] public GameObject HeldPrefab { get; private set; }

    [field: Header("Base Item Values")]
    [field: SerializeField] public int MinScienceValue { get; private set; }
    [field: SerializeField] public int MaxScienceValue { get; private set; }
    [field: SerializeField] public float MinMoneyValue { get; private set; }
    [field: SerializeField] public float MaxMoneyValue { get; private set; }
    [field: Header("Base Item Inventory Info")]
    [field: SerializeField] public bool IsPocketSize { get; private set; }
}
