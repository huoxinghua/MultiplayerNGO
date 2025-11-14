using System.Collections;
using _Project.ScriptableObjects.ScriptObjects.ItemSO.TranqGunItem;
using QuickOutline.Scripts;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem
{
    public class TranqGunInventoryItem : BaseInventoryItem
    {
        private NetworkVariable<int> AmmoLeft = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private TranqGunItemSO _tranqGunItemSO;
        public GameObject _bulletPrefab;
        public Transform _bulletSpawnPoint;
        public bool HasAmmoLeft => AmmoLeft.Value > 0;

        #region Setup + Update

        protected override void Awake()
        {
            base.Awake();
            if (_itemSO is TranqGunItemSO tranqGunItemSO)
            {
                _tranqGunItemSO = tranqGunItemSO;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Debug.Log("CustomNetworkSpawn called!");
            // Now add flashlight-specific network setup
            CustomNetworkSpawn();
            AmmoLeft = new NetworkVariable<int>(_tranqGunItemSO.AmmoAmount, NetworkVariableReadPermission.Everyone,
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
            if (!ItemCooldown.IsComplete)
                return;
            if (!HasAmmoLeft) return;
            if (IsOwner)
            {
                ShootGun();
            }
        }

        private void ShootGun()
        {
            /*_syringeItemSo.EffectDuration;
            _syringeItemSo.SpeedBoostAmount;*/
            Debug.Log("ShootGun");
            RequestDecreaseAmmoServerRpc();
            Instantiate(_bulletPrefab, _bulletSpawnPoint.position, Quaternion.LookRotation(transform.forward, Vector3.up));
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestDecreaseAmmoServerRpc()
        {
            AmmoLeft.Value--;
        }

        #endregion
    }
}


