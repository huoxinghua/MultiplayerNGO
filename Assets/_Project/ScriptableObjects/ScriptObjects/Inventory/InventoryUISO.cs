using UnityEngine;

[CreateAssetMenu(fileName = "InventoryUISO", menuName = "UI/InventoryUISO")]
public class InventoryUISO : ScriptableObject
{
    [field: SerializeField] public Color SelectedBackgroundColor { get; private set; }
    [field: SerializeField] public Color NonSelectedBackgroundColor { get; private set; }
}
