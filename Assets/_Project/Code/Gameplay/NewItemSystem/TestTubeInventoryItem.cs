using System.Collections;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.TestTubeItem;
using QuickOutline.Scripts;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem
{
    public class TestTubeInventoryItem : BaseInventoryItem
    {
        NetworkVariable<bool> HasCollected = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private TestTubeItemSO _testTubeItemSO;

        #region Setup + Update

        protected override void Awake()
        {
            base.Awake();
            if (_itemSO is TestTubeItemSO testTubeItemSO)
            {
                _testTubeItemSO = testTubeItemSO;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            HasCollected = new NetworkVariable<bool>(_testTubeItemSO.HasCollected, NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);
        }

        private void Update()
        {
            if (!IsOwner) return; // only the owning player updates
            UpdateHeldPosition();
        }

        #endregion

        #region UseLogic

        protected override bool CanUse()
        {
            if (!HasCollected.Value) return false;
            return base.CanUse();
        }

        protected override void ExecuteUsageLogic()
        {
            if (IsOwner)
            {
                UseTestTube();
            }
        }

        private void UseTestTube()
        {
            RequestChangeIsUsedServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestChangeIsUsedServerRpc()
        {
            HasCollected.Value = true;
        }

        #endregion
    
    }
}