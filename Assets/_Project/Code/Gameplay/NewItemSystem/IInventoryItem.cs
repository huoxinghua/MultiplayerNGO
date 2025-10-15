using UnityEngine;
using UnityEngine.UI;

public interface IInventoryItem 
{
    public void PickupItem(GameObject player, Transform playerHoldPosition);
    public void DropItem(Transform dropPoint);
    public void UseItem();
    public void UnequipItem();
    public void EquipItem();
    public string GetItemName();
    public bool IsPocketSize();
    public GameObject GetHeldVisual();
    public Image GetUIImage();
    public int GetSampleMonValue();
    public float GetSampleSciValue();
}
