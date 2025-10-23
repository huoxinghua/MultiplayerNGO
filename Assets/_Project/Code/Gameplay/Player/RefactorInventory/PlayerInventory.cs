using _Project.Code.Gameplay.FirstPersonController;
using _Project.Code.Gameplay.NewItemSystem;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.Player.RefactorInventory
{
    public class PlayerInventory : NetworkBehaviour
    {
        [SerializeField] public IInventoryItem[] InventoryItems = new IInventoryItem[5];
        public IInventoryItem BigItemCarried { get; private set; }
        [field: SerializeField] public int InventorySlots { get; private set; }
        [field: SerializeField] public Transform HoldTransform { get; private set; }
        [field: SerializeField] public Transform HoldTransformRPC { get; private set; }
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
        private void Start()
        {
             if (NetworkManager.Singleton != null)
       {
           Debug.Log($"NetworkManager active: " +
               $"IsServer={NetworkManager.Singleton.IsServer}, " +
               $"IsClient={NetworkManager.Singleton.IsClient}, " +
               $"IsHost={NetworkManager.Singleton.IsHost}, " +
               $"ConnectedClients={NetworkManager.Singleton.ConnectedClientsList.Count}");
       }
       else
       {
           Debug.Log("? NetworkManager is NULL");
       }
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
            var mono = item as MonoBehaviour;
            if (mono == null) return;

            var netObj = mono.GetComponent<NetworkObject>();
            if (netObj == null) return;

            Debug.Log($"[DoPickup] IsServer={IsServer}, IsOwner={IsOwner},IsClient ={IsClient}, RequireOwnership allowed, sending RPC...");
            if (IsServer)
            {
                Debug.Log("[DoPickup] IsServer HandlePickupOnServer");
                HandlePickupOnServer(item, OwnerClientId);
                return;
            }

            if (IsClient && IsOwner)
            {
                this.RequestPickupServerRpc(new NetworkObjectReference(netObj), OwnerClientId);
            }
            else
            {
                Debug.LogWarning("[DoPickup] Ignored (not owner or not client)");
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestPickupServerRpc(NetworkObjectReference itemRef, ulong playerId)
        {
            Debug.Log("RequestPickupServerRpc");
            if (!itemRef.TryGet(out NetworkObject netObj))
            {
                Debug.Log("!itemRef.TryGet(out NetworkObject netObj)");
                return;
            }

            var item = netObj.GetComponent<IInventoryItem>();
            if (item == null)
            {
                Debug.Log("netObj.GetComponent<IInventoryItem>() == null");
                return;
            }

            HandlePickupOnServer(item, playerId);
        }

        private void HandlePickupOnServer(IInventoryItem item, ulong playerId)
        {
            Debug.Log("HandlePickupOnServer PlayerID = "+playerId);
            if (item.IsPocketSize())
            {
                if (InventoryItems[_currentIndex] == null)
                {
                    InventoryItems[_currentIndex] = item;
                    item.PickupItem(gameObject, HoldTransform);
                    item.EquipItem();
                    Debug.Log("server side do Pice up");
                    TryNotifyClient(item);
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

            


        }
        private void TryNotifyClient(IInventoryItem item)
        {
            var mono = item as MonoBehaviour;
            if (mono == null)
            {
                Debug.LogWarning($"mono == null");
                return;
            }

            var netObj = mono.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                Debug.LogWarning($"NetworkObject missing on item");
                return;
            }

         
            NotifyClientsPickupClientRpc(new NetworkObjectReference(netObj), OwnerClientId);
        }
        [ClientRpc]
        private void NotifyClientsPickupClientRpc(NetworkObjectReference itemRef, ulong playerId)
        {
            if (NetworkManager.Singleton.LocalClientId == playerId)
            {
                Debug.Log($"[ClientRpc] LocalClientId ==({playerId}) skip.");
                return;
            }

            Debug.Log("NotifyClientsPickupClientRpc");
            if (!itemRef.TryGet(out NetworkObject netObj))
            {
                Debug.LogWarning("[ClientRpc] Invalid itemRef");
                return;
            }

            Debug.Log($"[ClientRpc] Updating visuals for {netObj.name} picked up by player {playerId}");

       
            var player = FindPlayerById(playerId);
            if (player == null)
            {
                Debug.LogWarning($"[ClientRpc] Player {playerId} not found!");
                return;
            }

          
            var inv = player.GetComponent<PlayerInventory>();
            if (inv == null)
            {
                Debug.LogWarning($"[ClientRpc]  player.GetComponent<PlayerInventory>()null!");
                return;
            }

            var renderer = netObj.GetComponent<Renderer>();
            if (renderer) renderer.enabled = false;

            var collider = netObj.GetComponent<Collider>();
            if (collider) collider.enabled = false;

            var rb = netObj.GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = true;

            var item = netObj.GetComponent<BaseInventoryItem>();
            if (item == null)
            {
                Debug.LogWarning("[ClientRpc] BaseInventoryItem missing on netObj!");
                return;
            }
           item.GenerateItemRPC(player,inv.HoldTransformRPC);
            var visual = item.GetHeldVisualRPC();
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            Debug.Log($"[ClientRpc] Spawned held visual for {player.name}");
            

            Debug.Log($"[ClientRpc] {netObj.name} visually attached to {player.name}");
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
    }
}
