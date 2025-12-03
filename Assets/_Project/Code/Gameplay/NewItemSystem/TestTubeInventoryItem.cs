using System.Collections;
using System.Collections.Generic;
using _Project.Code.Gameplay.NewItemSystem.SampleItem;
using _Project.Code.Gameplay.NewItemSystem.TestTub;
using _Project.Code.Gameplay.Scripts.MVCItems.SampleJar;
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
        private Dictionary<string, List<SampleData>> samplesContainer = new Dictionary<string, List<SampleData>>();
        [SerializeField] private float _detectDistance = 50f;
        [SerializeField] private LayerMask lM;
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

        public void CollectSample(SampleSO value)
        {
            Debug.Log("Save sample:" + value.name + "money:" + value.GetRandomMoneyValue() + "research" + value.GetRandomResearchValue());
            SampleData data = new SampleData(value.GetRandomResearchValue(),
                value.GetRandomMoneyValue());
            if (!samplesContainer.ContainsKey(value.SampleType))
            {
                samplesContainer[value.SampleType] = new List<SampleData>();
            }
            samplesContainer[value.SampleType].Add(data);
            Debug.Log("sample container:" + samplesContainer.Count);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestChangeIsUsedServerRpc()
        {
            HasCollected.Value = true;
         
        }

        #endregion
    
    }
}