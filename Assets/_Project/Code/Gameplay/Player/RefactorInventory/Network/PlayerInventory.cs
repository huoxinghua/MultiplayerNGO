using UnityEngine;
using Unity.Netcode;

namespace Project.Gameplay.Player.RefactorInventory.Network
{
    public class PlayerInventory : NetworkBehaviour
    {
        [SerializeField] public IInventoryItem[] InventoryItems = new IInventoryItem[5];
        public IInventoryItem BigItemCarried { get; private set; }
        [field: SerializeField] public int InventorySlots { get; private set; }
        [field: SerializeField] public Transform HoldTransform { get; private set; }
        [field: SerializeField] public Transform DropTransform { get; private set; }
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
            for (int i = 0; i < InventoryItems.Length; i++)
            {
                if (InventoryItems[i] == null)
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
                    item.PickupItem(gameObject, HoldTransform);
                    item.EquipItem();
                }
                else
                {
                    //should find first available slot? I hope
                    for (int i = 0; i < InventoryItems.Length; i++)
                    {
                        var items = InventoryItems[i];
                        if (items == null)
                        {
                            InventoryItems[i] = item;
                            item.PickupItem(gameObject, HoldTransform);
                            item.UnequipItem();
                            break;
                        }

                    }
                }
                //add to inventory list
            }
            else
            {
                BigItemCarried = item;
                InventoryItems[_currentIndex]?.UnequipItem();
                item.PickupItem(gameObject, HoldTransform);
                item.EquipItem();
            }
            if (item is MonoBehaviour mono && mono.TryGetComponent(out NetworkObject netObj))
            {
                HideItemServerRpc(netObj.NetworkObjectId);
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
            if (_handsFull)
            {
                BigItemCarried?.UseItem();
            }
            else
            {
                InventoryItems[_currentIndex]?.UseItem();
            }

        }
        /// <summary>
        /// Checks if holding a sample in hand. Safety net for TrySell()
        /// </summary>
        /// <returns>If item held can be sold</returns>
        public bool IsHoldingSample()
        {
            if (_handsFull)
            {
                if (BigItemCarried != null)
                {
                    return BigItemCarried.CanBeSold();
                }
            }
            else if (InventoryItems[_currentIndex] != null)
            {
                return InventoryItems[_currentIndex].CanBeSold();
            }
            return false;
        }
        /// <summary>
        /// Checks if the applicable item is eligible for sale. If holding big item, check big item. If not, check for a held item and if it can be sold
        /// </summary>
        /// <returns>A struct containing the data associated with the samples values</returns>
        public ScienceData TrySell()
        {
            float tranquilVal = 0;
            float violentVal = 0;
            float miscVal = 0;
            string itemName = "StoopidDumb";
            //big item check and logic
            if (_handsFull && BigItemCarried != null)
            {

                tranquilVal = BigItemCarried.GetValueStruct().RawTranquilValue;
                violentVal = BigItemCarried.GetValueStruct().RawViolentValue;
                miscVal = BigItemCarried.GetValueStruct().RawMiscValue;
                itemName = BigItemCarried.GetValueStruct().KeyName;
                BigItemCarried.WasSold();
                BigItemCarried = null;
            }
            //inventory check and item held check (if item is held)
            else if (!_handsFull && InventoryItems[_currentIndex] != null)
            {
                if (InventoryItems[_currentIndex] == null) return new ScienceData { RawTranquilValue = tranquilVal, RawViolentValue = violentVal, RawMiscValue = miscVal };
                tranquilVal = InventoryItems[_currentIndex].GetValueStruct().RawTranquilValue;
                violentVal = InventoryItems[_currentIndex].GetValueStruct().RawViolentValue;
                miscVal = InventoryItems[_currentIndex].GetValueStruct().RawMiscValue;
                itemName = InventoryItems[_currentIndex].GetValueStruct().KeyName;
                InventoryItems[_currentIndex].WasSold();
                InventoryItems[_currentIndex] = null;
            }

            return new ScienceData { RawTranquilValue = tranquilVal, RawViolentValue = violentVal, RawMiscValue = miscVal, KeyName = itemName };
        }
        /// <summary>
        /// Called by number inputs to switch to new slot. 
        /// <br/>Tells current item (if applicable) to handle unequip logic, sets new index for current item, than tells new item to handle equip logic
        /// </summary>
        /// <param name="indexOf"> The index of the inventory slot to switch to </param>
        private void EquipSlot(int indexOf)
        {
            if (_handsFull) return;
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

        [ServerRpc(RequireOwnership = false)]
        private void HideItemServerRpc(ulong itemNetworkId)
        {
            HideItemClientRpc(itemNetworkId);
        }

        [ClientRpc]
        private void HideItemClientRpc(ulong itemNetworkId)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(itemNetworkId, out var netObj))
            {
                netObj.gameObject.SetActive(false);
            }
        }
    }

}