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
            if (!ItemCooldown.IsComplete)
                return;
            if (!HasAmmoLeft) return;
            if (IsOwner)
            {
                ShootGun();
            }
            base.UseItem();
        }

        private void ShootGun()
        {
            /*_syringeItemSo.EffectDuration;
            _syringeItemSo.SpeedBoostAmount;*/
            Debug.Log("ShootGun");
            RequestDecreaseAmmoServerRpc();
            var cam = Camera.main;
            Vector3 shootDir;
            if (cam != null)
            {
                Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                shootDir = ray.direction;

           
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                    shootDir = (hit.point - _bulletSpawnPoint.position).normalized;
            }
            else
            {
                shootDir = transform.forward; 
            }
            var dartObj = Instantiate(_bulletPrefab, _bulletSpawnPoint.position,
                Quaternion.LookRotation(shootDir));

            //var dartObj = Instantiate(_bulletPrefab, _bulletSpawnPoint.position, Quaternion.LookRotation(transform.forward, Vector3.up));
            var dartScript =dartObj.GetComponent<TranqDartScript>();
            if (dartScript != null)
            {
                dartScript.Owner = _owner;
                Debug.Log("gun owner:"+  dartScript.Owner);
                dartScript.SetVelocity(shootDir); 
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


