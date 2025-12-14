using System.Collections;
using System.Collections.Generic;
using _Project.Code.Gameplay.Market.Sell;
using _Project.Code.Gameplay.NewItemSystem.SampleItem;
using _Project.Code.Utilities.EventBus;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.TestTubeItem;
using QuickOutline.Scripts;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem
{
    public enum SampleType
    {
        NoSample,
        BruteSample,
        BeetleSample,
        DollSample
    }
    public class TestTubeInventoryItem : BaseInventoryItem
    {
        NetworkVariable<bool> HasCollected = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private NetworkVariable<SampleType> ThisSampleTypeNet = new NetworkVariable<SampleType>(SampleType.NoSample,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);   
        private TestTubeItemSO _testTubeItemSO;
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

            HasCollected = new NetworkVariable<bool>(_testTubeItemSO.HasCollected,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);
            // SERVER ONLY: Generate random values on spawn
            if (IsServer)
            {
                SellableItemManager.Instance?.RegisterItem(NetworkObject, this);
            }

            // Register callbacks to update local cache when values change
            _tranquilValueNet.OnValueChanged += (oldVal, newVal) => _tranquilValue = newVal;
            _violentValueNet.OnValueChanged += (oldVal, newVal) => _violentValue = newVal;
            _miscValueNet.OnValueChanged += (oldVal, newVal) => _miscValue = newVal;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsServer)
            {
                SellableItemManager.Instance?.UnregisterItem(NetworkObject);
            }
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
                        EventBus.Instance.PublishGameplayEvent(new TestTubeCollectEvent
                        {
                            EventID = EventIDs.ItemTestTubeCollect, EventPosition = _fpsHeldVisualChild.transform.position
                        });
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

        [ServerRpc(RequireOwnership = false)]
        private void SetSampleValuesServerRpc(float newTranqVal, float newViolentVal,float newMiscVal, SampleType sampleType)
        {
            HasCollected.Value = true;
            _tranquilValueNet.Value = newTranqVal;
            _violentValueNet.Value = newViolentVal;
            _miscValueNet.Value = newMiscVal;
            ThisSampleTypeNet.Value = sampleType;
        }
        public void CollectSample(SampleSO value)
        {
            // SERVER ONLY: Generate random values on spawn
            if (IsServer)
            {
                SetSampleValuesServerRpc(value.GetRandomTranquilValue(), value.GetRandomViolentValue(), 
                    value.GetRandomMiscValue(), value.SampleType);

            }

            //!!!!! Critically bad. SampleData takes a tranquil, violent, than a misc in that order. This gives a misc and 
            //two violents. Wrong order, missing tranquil
            /*SampleData data = new SampleData(value.GetRandomMiscValue(),
                value.GetRandomViolentValue(), value.GetRandomViolentValue());*/
            //!! new way to do this. Wont use container. Single use item
            /*if (!samplesContainer.ContainsKey(value.SampleType))
            {
                samplesContainer[value.SampleType] = new List<SampleData>();
            }
            samplesContainer[value.SampleType].Add(data);*/
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestChangeIsUsedServerRpc()
        {
            HasCollected.Value = true;
        }

        #endregion

        #region Overrides

        public override bool CanBeSold()
        {
            return HasCollected.Value;
        }
        public override ScienceData GetValueStruct()
        {
            return new ScienceData
            {
                RawTranquilValue = _tranquilValueNet.Value,
                RawViolentValue = _violentValueNet.Value,
                RawMiscValue = _miscValueNet.Value,
                KeyName = ThisSampleTypeNet.Value.ToString()
            };
        }
        #endregion
    }
}