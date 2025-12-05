using System.Collections;
using System.Collections.Generic;
using _Project.Code.Gameplay.NewItemSystem.SampleItem;
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

        protected override void ExecuteUsageLogic()
        {
            if (IsOwner)
            {
                UseTestTube();
            }
        }
        private void UseTestTube()
        {
            // 1. Only Owner should Raycast
            if (!IsOwner) return;

            var cam = Camera.main;
            if (cam == null) return;

            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, _detectDistance, lM))
            {

                var sample = hit.transform.GetComponent<SampleObjTest>();
                if (sample != null)
                {
                    // Get data
                    var sampleSO = sample.GetSample();

                    // Local save
                    CollectSample(sampleSO);

                    // Tell server this sample is collected → destroy networked object
                    var netObj = sample.GetComponent<NetworkObject>();
                    if (netObj != null)
                    {
                        RequestCollectSampleServerRpc(new NetworkObjectReference(netObj));
                    }
                    else
                    {
                        Debug.LogError("Sample missing NetworkObject");
                    }

                    return;
                }
            }

        }
        [ServerRpc(RequireOwnership = false)]
        private void RequestCollectSampleServerRpc(NetworkObjectReference sampleObjRef)
        {
            if (sampleObjRef.TryGet(out NetworkObject obj))
            {
                obj.Despawn();
            }

            HasCollected.Value = true;
        }
        public void CollectSample(SampleSO value)
        {
            SampleData data = new SampleData(value.GetRandomMiscValue(),
                value.GetRandomViolentValue(), value.GetRandomViolentValue());
            if (!samplesContainer.ContainsKey(value.SampleType))
            {
                samplesContainer[value.SampleType] = new List<SampleData>();
            }
            samplesContainer[value.SampleType].Add(data);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestChangeIsUsedServerRpc()
        {
            HasCollected.Value = true;
         
        }

        #endregion
    
    }
}