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
        public Transform _bulletSpawnPoint; // This should be assigned in the prefab, as a child of the FPS visual
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

        protected override bool CanUse()
        {
            if (!HasAmmoLeft) return false;
            return base.CanUse();
        }

        protected override void ExecuteUsageLogic()
        {
            if (!IsOwner) return;
            
            if (_bulletSpawnPoint == null)
            {
                Debug.LogError("Bullet spawn point not assigned on TranqGunInventoryItem prefab!");
                return;
            }

            Vector3 spawnPosition = _bulletSpawnPoint.position;
            Quaternion spawnRotation = _bulletSpawnPoint.rotation;
            Vector3 shootDir = GetAimDirection(spawnPosition);
            
            ShootServerRpc(spawnPosition, spawnRotation, shootDir);
        }

        private Vector3 GetAimDirection(Vector3 spawnPoint)
        {
            var cam = Camera.main;
            Vector3 shootDir;
            if (cam != null)
            {
                Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                shootDir = ray.direction;


                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                    shootDir = (hit.point - spawnPoint).normalized;
            }
            else
            {
                shootDir = transform.forward;
            }

            return shootDir;
        }

        [ServerRpc]
        private void ShootServerRpc(Vector3 spawnPos, Quaternion spawnRot, Vector3 aimDir)
        {
            RequestDecreaseAmmoServerRpc();

      
            var dartObj = Instantiate(_bulletPrefab, spawnPos, spawnRot);
            var netObj = dartObj.GetComponent<NetworkObject>();
            netObj.Spawn();   
            var dartScript = dartObj.GetComponent<TranqDartScript>();
            if (dartScript != null)
            {
                dartScript.Owner = _owner;
                dartScript.SetVelocity(aimDir);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestDecreaseAmmoServerRpc()
        {
            AmmoLeft.Value--;
        }

        #endregion
    }
}