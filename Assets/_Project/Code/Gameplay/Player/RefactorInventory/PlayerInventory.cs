using _Project.Code.Gameplay.FirstPersonController;
using _Project.Code.Gameplay.NewItemSystem;
using _Project.Code.Gameplay.Player.UsableItems;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Gameplay.Player.RefactorInventory
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
        [field: SerializeField] public Image[] SlotDisplay { get; private set; } = new Image[5];
        [field: SerializeField] public Image[] SlotBackground { get; private set; } = new Image[5];
        [field: SerializeField] public Image EmptySlot { get; private set; }
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
        // not setup or tested
        public void ChangeUISlotDisplay()
        {
            return;
            for (int i = 0; i < InventoryItems.Length; i++)
            {
                if (InventoryItems[i] == null)
                {
                    SlotDisplay[i] = EmptySlot;
                }
                else
                {
                    SlotDisplay[i] = InventoryItems[i].GetUIImage();
                }
            }
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
                    item.PickupItem(gameObject, HoldTransform, NetworkObject);
                    item.EquipItem();
                    Debug.Log("server side do Pice up");
                    //network


                    ChangeSlotBackgrounds(_currentIndex);
                    //end network
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
                            item.PickupItem(gameObject, HoldTransform,NetworkObject);
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
                item.PickupItem(gameObject, HoldTransform, NetworkObject);
                item.EquipItem();
                ChangeSlotBackgrounds(-1);
            }
            ChangeUISlotDisplay();



        }

        /*  [ClientRpc]
          private void NotifyClientsPickupClientRpc(NetworkObjectReference itemRef, ulong playerId)
          {
              if (NetworkManager.Singleton.LocalClientId == playerId)
              {
                  Debug.Log($"[ClientRpc] Skipping self ({playerId}) visual update.");
                  return;
              }

              Debug.Log("NotifyClientsPickupClientRpc");
              if (!itemRef.TryGet(out NetworkObject netObj)) return;

              var item = netObj.GetComponent<MonoBehaviour>() as IInventoryItem;
              if (item == null) return;

              Debug.Log($"[Sync] Item picked up by player {playerId}");


              var player = FindPlayerById(playerId);
              if (player == null)
              {
                  Debug.LogWarning($"[ClientRpc] Player {playerId} not found.");
                  return;
              }
              var inventory = player.GetComponent<PlayerInventory>();
              if (inventory == null)
              {
                  Debug.LogWarning($"[ClientRpc] Player {playerId} has no PlayerInventory!");
                  return;
              }
              netObj.transform.SetParent(inventory.HoldTransform);
              netObj.transform.localPosition = Vector3.zero;
              netObj.transform.localRotation = Quaternion.identity;
              netObj.gameObject.SetActive(true);
              Debug.Log($"[ClientRpc] {netObj.name} attached to {player.name}'s hand");
          }
          private GameObject FindPlayerById(ulong clientId)
          {
              foreach (var obj in Object.FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
              {
                  if (obj.IsPlayerObject && obj.OwnerClientId == clientId)
                  {
                      return obj.gameObject;
                  }
              }
              return null;
          }*/

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
            ChangeUISlotDisplay();
        }
        
        //not setup or tested
        public void ChangeSlotBackgrounds(int selectedItem)
        {
            return;
            for (int i = 0; i < SlotDisplay.Length; i++)
            {
                if (i == selectedItem)
                {
                    SlotBackground[i].color = Color.red;
                }
                else
                {
                    SlotBackground[i].color = Color.white;
                }
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
            ChangeUISlotDisplay();
            ChangeSlotBackgrounds(_currentIndex);
        }
        #region Inputs
        private void HandlePressedSlot(int index)
        {
            if (_handsFull) return;
            EquipSlot(index - 1);
        }

        #endregion
    }
}
