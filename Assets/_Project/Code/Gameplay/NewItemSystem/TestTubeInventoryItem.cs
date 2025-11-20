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
        private float _detectDistance = 50f;
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

        public override void UseItem()
        {
            base.UseItem();
            if (HasCollected.Value) return;
            if (IsOwner)
            {
                UseTestTube();
            }
            else
            {
                Debug.Log("UseItem：not owner");
            }
          
        }

        private void UseTestTube()
        {
            /*_testTubeItemSo.EffectDuration;
            _testTubeItemSo.SpeedBoostAmount;*/

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, _detectDistance,lM))
            {
                var sample = hit.transform.GetComponent<SampleObjTest>();
                if (sample != null)
                {
                    var currentSample = sample.GetSample();
                    CollectSample(currentSample);
                }
                if (sample.gameObject != null)
                {
                    Destroy(sample.gameObject);
                }

            }
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
