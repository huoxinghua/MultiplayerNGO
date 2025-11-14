using _Project.Code.Network.RegisterNetObj;
using _Project.Code.Utilities.Audio;
using _Project.Code.Utilities.Utility;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.Interactables.Network
{
    public class SwingDoors : NetworkBehaviour, IInteractable
    {
        //private bool _isOpen = false;
        private NetworkVariable<bool> _isOpen = new NetworkVariable<bool>(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private bool _openedByEnemy = false;
        private Timer _enemyOpenedTimer = new Timer(0);
        [SerializeField] private float _enemyCloseDelay;

        private void OnEnable()
        {
            _isOpen.OnValueChanged += OnDoorStateChanged;
        }
        private void Disable()
        {
            _isOpen.OnValueChanged -= OnDoorStateChanged;
        }

        private void OnDoorStateChanged(bool oldValue, bool newValue)
        {
            ApplyDoorRotation(newValue);
        }
        private void ApplyDoorRotation(bool isOpen)
        {
            transform.localRotation = Quaternion.Euler(0f, isOpen ? 90f : 0f, 0f);
        }

        public void OnInteract(GameObject interactingPlayer)
        {
            Debug.Log("IsServer" + IsServer);
            if (!IsServer)
            {
                RequestToggleServerRpc();
            }
            else
            {
                ToggleOpen();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestToggleServerRpc(ServerRpcParams rpcParams = default)
        {
            ToggleOpen();
        }

        public bool IsDoorOpen() { return _isOpen.Value; }
        public void EnemyOpened()
        {
            if (_isOpen.Value) return;
            ToggleOpen();
            _enemyOpenedTimer.Reset(_enemyCloseDelay);
            _openedByEnemy = true;
        }
        /*public void EnemyOpened()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                if (_isOpen.Value) return;
                ToggleOpen();
                _enemyOpenedTimer.Reset(_enemyCloseDelay);
                _openedByEnemy=true;
            }
            else
            {
                RequestEnemyOpenServerRpc();
            }
        }
        [ServerRpc(RequireOwnership = false)]
        private void RequestEnemyOpenServerRpc()
        {
            Debug.Log("enemy open door server Rpc");
            if (_isOpen.Value) return;
            ToggleOpen();

            _isOpen.Value = true;
            _enemyOpenedTimer.Reset(_enemyCloseDelay);
            _openedByEnemy = true;
        }*/

        public void ToggleOpen()
        {
            if (!IsServer)
                return; 
          //  transform.localRotation = Quaternion.Euler(0f, _isOpen.Value ? 0f : 90f, 0f);
            _isOpen.Value = !_isOpen.Value;
            AudioManager.Instance.PlayByKey3D("DoorOpen", transform.position);
        }

        // Update is called once per frame
        void Update()
        {
            _enemyOpenedTimer?.TimerUpdate(Time.deltaTime);
            if (_enemyOpenedTimer.IsComplete && _openedByEnemy && _isOpen.Value)
            {
                ToggleOpen();
                _openedByEnemy = false;
            }
        }

    }
}