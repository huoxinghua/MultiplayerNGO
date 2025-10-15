using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] public IInventoryItem[] InventoryItems = new IInventoryItem[5];
    public IInventoryItem BigItemCarried { get; private set; }
    [field: SerializeField] public int InventorySlots {  get; private set; }
    [field: SerializeField] public Transform HoldTransform { get; private set; }
    [field: SerializeField] public Transform DropTransform {  get; private set; }
    private int _currentIndex;
    [SerializeField] private PlayerInputManager _inputManager;
    private bool _handsFull => BigItemCarried != null;
    public bool InventoryFull => IsInventoryFull();
    public void Awake()
    {
        _inputManager.OnNumPressed += HandlePressedSlot;


        _inputManager.OnUse += UseItemInHand;
        _inputManager.OnDropItem += DropItem;
    }
    public void OnDisable()
    {
        _inputManager.OnNumPressed -= HandlePressedSlot;
        

        _inputManager.OnUse -= UseItemInHand;
        _inputManager.OnDropItem -= DropItem;
    }

    public bool IsInventoryFull()
    {
        bool isFull = true;
        for(int i = 0; i < InventoryItems.Length; i++)
        {
            if(InventoryItems[i] == null)
            {
                isFull = false;
            }
        }
        return isFull;
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
                item.PickupItem(gameObject,HoldTransform);
                item.EquipItem();
            }
            else
            {
                //should find first available slot? I hope
                for(int i = 0; i < InventoryItems.Length; i++)
                {
                    var items = InventoryItems[i];
                    if(items == null)
                    {
                        InventoryItems[i] = item;
                        item.PickupItem(gameObject, HoldTransform);
                        break;
                    }
       
                }
            }
            //add to inventory list
        }
        else
        {
            BigItemCarried = item;
            item.PickupItem(gameObject, HoldTransform);
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
            BigItemCarried.DropItem(DropTransform);
            BigItemCarried = null;
        }
        else if (InventoryItems[_currentIndex] != null)
        {
            //first unequip to hide held visual
            //than handle drop to make it visible as an in scene item
            //than set null
            InventoryItems[_currentIndex].UnequipItem();
            InventoryItems[_currentIndex].DropItem(DropTransform);
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
    private void HandlePressedSlot(int index)
    {
        if (_handsFull) return;
        EquipSlot(index - 1);
    }
  
    #endregion
}
