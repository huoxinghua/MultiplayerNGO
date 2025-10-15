using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public List<IInventoryItem> InventoryItems = new List<IInventoryItem>();
    public IInventoryItem BigItemCarried { get; private set; }
    [field: SerializeField] public int InventorySlots {  get; private set; }
    private int _currentIndex;
    [SerializeField] private PlayerInputManager _inputManager;
    private bool _handsFull => BigItemCarried != null;
    public bool InventoryFull => InventoryItems.Count >= InventorySlots;
    public void Awake()
    {
        _inputManager.OnNumOne += HandlePressedSlotOne;
        _inputManager.OnNumTwo += HandlePressedSlotTwo;
        _inputManager.OnNumThree += HandlePressedSlotThree;
        _inputManager.OnNumFour += HandlePressedSlotFour;
        _inputManager.OnNumFive += HandlePressedSlotFive;

        _inputManager.OnUse += UseItemInHand;
        _inputManager.OnDropItem += DropItem;
    }
    public void OnDisable()
    {
        _inputManager.OnNumOne -= HandlePressedSlotOne;
        _inputManager.OnNumTwo -= HandlePressedSlotTwo;
        _inputManager.OnNumThree -= HandlePressedSlotThree;
        _inputManager.OnNumFour -= HandlePressedSlotFour;
        _inputManager.OnNumFive -= HandlePressedSlotFive;

        _inputManager.OnUse -= UseItemInHand;
        _inputManager.OnDropItem -= DropItem;
    }



    /// <summary>
    /// Called by interact cast. When interacting with an item than can be picked up, this function checks if thats possible
    /// </summary>
    /// <returns>True if possible to pickup, false if not</returns>
    public bool TryPickupItem()
    {
        if (_handsFull || InventoryFull) return false;
        return true;
    }


    /// <summary>
    /// Handles the pickup logic. Adds IInventory item to inventory. Equips to hand if current slot free. 
    /// <br/>Else adds to closest slot to the left. if item isnt pocket sized, place in hand not inventory
    /// </summary>
    /// <param name="item"> the item itself being picked up</param>
    public void DoPickup(IInventoryItem item)
    {
        if (item.IsPocketSize())
        {
            if (InventoryItems[_currentIndex] == null) 
            {
                InventoryItems[_currentIndex] = item;
                item.PickupItem();
                item.EquipItem();
            }
            else
            {
                //should find first available slot? I hope
                int i = 0;
                foreach(var items in InventoryItems)
                {
                    if(items == null)
                    {
                        InventoryItems[i] = item;
                        item.PickupItem();
                        break;
                    }
                    i++;
                }
            }
            //add to inventory list
        }
        else
        {
            BigItemCarried = item;
            item.PickupItem();
            item.EquipItem();
        }
    }


    /// <summary>
    /// Tells items at current item index to handle drop logic. This is done via input
    /// </summary>
    public void DropItem()
    {
        if (_handsFull)
        {
            //first unequip to hide held visual
            //than handle drop to make it visible as an in scene item
            //than set null
            BigItemCarried.UnequipItem();
            BigItemCarried.DropItem();
            BigItemCarried = null;
        }
        else if (InventoryItems[_currentIndex] != null)
        {
            //first unequip to hide held visual
            //than handle drop to make it visible as an in scene item
            //than set null
            InventoryItems[_currentIndex].UnequipItem();
            InventoryItems[_currentIndex].DropItem();
            InventoryItems[_currentIndex] = null;
            //drop current slot if there is one
        }
    }
    public void UseItemInHand()
    {
        if(_handsFull)
        {
            BigItemCarried?.UseItem();
        }
        else
        {
            InventoryItems[_currentIndex]?.UseItem();
        }
        
    }

    /// <summary>
    /// Called by number inputs to switch to new slot. 
    /// <br/>Tells current item (if applicable) to handle unequip logic, sets new index for current item, than tells new item to handle equip logic
    /// </summary>
    /// <param name="indexOf"> The index of the inventory slot to switch to </param>
    private void EquipSlot(int indexOf)
    {
        if (InventoryItems[_currentIndex] != null) 
        {
            InventoryItems[_currentIndex].UnequipItem();
        }
        _currentIndex = indexOf;
        InventoryItems[_currentIndex]?.EquipItem();
    }
    #region Inputs
    public void HandlePressedSlotOne()
    {
        if (_handsFull) return;
        EquipSlot(0);
    }
    public void HandlePressedSlotTwo()
    {
        if (_handsFull) return;
        EquipSlot(1);
    }
    public void HandlePressedSlotThree()
    {
        if (_handsFull) return;
        EquipSlot(2);
    }
    public void HandlePressedSlotFour()
    {
        if (_handsFull) return;
        EquipSlot(3);
    }
    public void HandlePressedSlotFive()
    {
        if (_handsFull) return;
        EquipSlot(4);
    }
    #endregion
}
