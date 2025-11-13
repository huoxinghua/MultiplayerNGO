using System.Collections;
using _Project.Code.Art.AnimationScripts.Animations;
using _Project.Code.Art.AnimationScripts.IK;
using _Project.Code.Gameplay.NewItemSystem;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Gameplay.Player.UsableItems;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.Gameplay.Player.RefactorInventory
{
    public class PlayerInventory : NetworkBehaviour
    {
        [SerializeField] public BaseInventoryItem[] InventoryItems = new BaseInventoryItem[5];
        public NetworkList<NetworkObjectReference> InventoryNetworkRefs =  new NetworkList<NetworkObjectReference>();
        public NetworkVariable<int> NetworkCurrentIndex = new NetworkVariable<int>();
        public NetworkVariable<NetworkObjectReference> InventoryNetworkBigItemRef;
        public BaseInventoryItem BigItemCarried { get; private set; }
        [field: SerializeField] public int InventorySlots { get; private set; }
        [field: SerializeField] public Transform HoldTransform { get; private set; }
        [field: SerializeField] public Transform DropTransform { get; private set; }
        private int _currentIndex;
        [SerializeField] private PlayerInputManager _inputManager;
        [field: SerializeField] public Image[] SlotDisplay { get; private set; } = new Image[5];
        [field: SerializeField] public Image[] SlotBackground { get; private set; } = new Image[5];
        [field: SerializeField] public Image EmptySlot { get; private set; }
        /*[field: SerializeField] public PlayerIKController  PlayerFPSIKController { get; private set; }
        [field: SerializeField] public PlayerIKController PlayerTPSIKController { get; private set; }*/
        [field: SerializeField] public PlayerAnimation PlayerAnimation { get; private set; }
        [field: SerializeField] public PlayerIKData ThisPlayerIKData { get; private set; }
        private bool _handsFull => BigItemCarried != null;
        public bool InventoryFull => IsInventoryFull();
        
        #region Setup + Update
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            for (int i = 0; i < InventoryItems.Length; i++)
            {
                RequestAddToListServerRpc();
            }
            InventoryNetworkRefs.OnListChanged += HandleInventoryListChange;
            NetworkCurrentIndex.OnValueChanged += HandleCurrentIndexChange;
            RequestChangeCurrentIndexServerRpc(0);
        }
        
        public void Awake()
        {
            _inputManager.OnNumPressed += HandlePressedSlot;
            _inputManager.OnUse += UseItemInHand;
            _inputManager.OnSecondaryUse += SecondaryUseItemInHand;
            _inputManager.OnDropItem += DropItem;
            
        }
        public void OnDisable()
        {
            _inputManager.OnNumPressed -= HandlePressedSlot;


            _inputManager.OnUse -= UseItemInHand;
            _inputManager.OnDropItem -= DropItem;
        }
        #endregion
        
        #region Pickup Logic
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
        public void DoPickup(BaseInventoryItem item)
        {
            if (item.IsPocketSize())
            {
                //For pocket size items and when the current slot is available
                if (InventoryItems[_currentIndex] == null)
                {
                    item.PickupItem(gameObject, HoldTransform, NetworkObject);
                    item.EquipItem();
                    Debug.Log("server side do Pice up");
                    //network
                    RequestReplaceAtListServerRpc(_currentIndex, new NetworkObjectReference(item.NetworkObject));
                    ChangeSlotBackgrounds(_currentIndex);
                    //end network
                }
                //For pocket size items and when current slot isnt available - Finds first available slot
                else
                {
                    //should find first available slot? I hope
                    for (int i = 0; i < InventoryItems.Length; i++)
                    {
                        var items = InventoryItems[i];
                        if (items == null)
                        {
                            //InventoryItems[i] = item;
                            item.PickupItem(gameObject, HoldTransform,NetworkObject);
                            item.UnequipItem();
                            break;
                        }
                    }
                }
            }
            //for non pocketsize items. Puts them in hand
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
          
        #endregion
        
        #region Drop Logic
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
                RequestReplaceAtListServerRpc(_currentIndex,new NetworkObjectReference(NetworkObject));
                
                //drop current slot if there is one
            }
            ChangeUISlotDisplay();
        }
        #endregion
        
        #region Swap Logic
        
        public void HandleCurrentIndexChange(int oldInt, int newInt)
        {
            _currentIndex = newInt;
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
            RequestChangeCurrentIndexServerRpc(_currentIndex);
            ChangeUISlotDisplay();
            ChangeSlotBackgrounds(_currentIndex);
            
        }
        #endregion
        
        #region Use Logic
        public void UseItemInHand()
        {
            if(InventoryItems[_currentIndex] != null && InventoryItems[_currentIndex].ItemCooldown.IsComplete)
                PlayerAnimation.PlayInteract();
            if (_handsFull)
            {
                BigItemCarried?.UseItem();
            }
            else
            {
                InventoryItems[_currentIndex]?.UseItem();
            }
           
        }
        public void SecondaryUseItemInHand(bool isPerformed)
        {
            if (_handsFull)
            {
                BigItemCarried?.SecondaryUse(isPerformed);
            }
            else
            {
                InventoryItems[_currentIndex]?.SecondaryUse(isPerformed);
            }
        }
        #endregion
        
        #region NetVar Logic
        [ServerRpc(RequireOwnership = false)]
        void RequestChangeCurrentIndexServerRpc(int newIndex)
        {
            NetworkCurrentIndex.Value = newIndex;
        }
        [ServerRpc(RequireOwnership = false)]
        void RequestAddToListServerRpc()
        {
            InventoryNetworkRefs.Add(new NetworkObjectReference(NetworkObject));
        }
        [ServerRpc(RequireOwnership = false)]
        void RequestReplaceAtListServerRpc(int index, NetworkObjectReference reference)
        {
            InventoryNetworkRefs[index] = reference;
        }
        public void HandleInventoryListChange(NetworkListEvent<NetworkObjectReference> netlistEvent)
        {
            for (int i = 0; i < InventoryItems.Length; i++)
            {
                if (InventoryNetworkRefs[i].TryGet(out NetworkObject itemNetObj))
                {
                    InventoryItems[i] = itemNetObj.GetComponent<BaseInventoryItem>();
                }
            }
        }
        #endregion

        #region Inventory Slot Logic
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
        
        
        #endregion
        
        #region  UI Logic
        
        
        //UI LOGIC IS NOT TESTED, SETUP, OR ANY GOOD. Ignore for now...
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

        #endregion
        
        #region Sell Logic

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
        #endregion
        
        #region Junk
        /*IEnumerator WaitForCurrentHeldDropAgain(int index)
      {
          yield return new WaitUntil(() =>
          {
              if (InventoryItems == null)
                  return false;

              if (index < 0 || index >= InventoryItems.Length)
                  return false;

              var item = InventoryItems[index];
              if (item == null)
                  return false;

              var ik = item.GetIKInteractable();
              return ik != null;
          });
      }*/
        #endregion
        
        #region Inputs
        private void HandlePressedSlot(int index)
        {
            if (_handsFull) return;
            EquipSlot(index - 1);
        }

        #endregion
    }
}
