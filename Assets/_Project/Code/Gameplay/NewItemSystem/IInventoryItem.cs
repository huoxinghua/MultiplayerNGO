using UnityEngine;

public interface IInventoryItem 
{
    public void PickupItem();
    public void DropItem();
    public void UseItem();
    public void UnequipItem();
    public void EquipItem();
    public string GetItemName();
    public bool IsPocketSize();
}
