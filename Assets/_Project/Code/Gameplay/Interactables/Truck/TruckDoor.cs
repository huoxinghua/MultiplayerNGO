using _Project.Code.Network.GameManagers;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.Interactables.Truck
{
    public class TruckDoor : NetworkBehaviour, IInteractable
    {
        [Header("Open Direction Settings")]
        [SerializeField] private float openAngle = -110f;

        [SerializeField] private float speed = 3f;

        private readonly NetworkVariable<bool> _openState = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private Quaternion _initialRotation;
        private float _currentAngle;
        private bool _isOpen;

        private void Awake()
        {
            _initialRotation = transform.localRotation;
            _currentAngle = 0f;
            _isOpen = false;
        }

        public override void OnNetworkSpawn()
        {
            _isOpen = _openState.Value;
            _openState.OnValueChanged += OnDoorStateChanged;
        }

        public override void OnNetworkDespawn()
        {
            _openState.OnValueChanged -= OnDoorStateChanged;
        }

        private void Update()
        {
            var targetAngle = _isOpen ? openAngle : 0f;
            _currentAngle = Mathf.Lerp(_currentAngle, targetAngle, Time.deltaTime * speed);
            transform.localRotation = _initialRotation * Quaternion.Euler(0f, _currentAngle, 0f);
        }

        public void OnInteract(GameObject player)
        {
            if (IsServer)
            {
                ToggleDoor();
            }
            else
            {
                ToggleDoorServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void ToggleDoorServerRpc()
        {
            ToggleDoor();
        }

        private void ToggleDoor()
        {
            if (!IsServer)
                return;
            Debug.Log("open Door");

            _openState.Value = !_openState.Value;
            GameFlowManager.Instance.StartMission("SecondShowcase_v3_XH");
        }

        private void OnDoorStateChanged(bool previous, bool next)
        {
            _isOpen = next;
        }
    }
}
