using _Project.Code.Gameplay.NewItemSystem;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute
{
    public class BrutePiece : BaseInventoryItem
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Debug.Log("CustomNetworkSpawn called!");

            CustomNetworkSpawn();
        }
        public void Awake()
        {
            _tranquilValue = Random.Range(0f, 1f);
            _violentValue = Random.Range(0f, 1f);
            _miscValue = Random.Range(0f, 1f);
        }
        private void Update()
        {
            if (!IsOwner) return;
            UpdateHeldPosition();

        }
        protected override void UpdateHeldPosition()
        {
            if (_currentHeldVisual == null || CurrentHeldPosition == null) return;
            _currentHeldVisual.transform.position = CurrentHeldPosition.position;
            _currentHeldVisual.transform.rotation = CurrentHeldPosition.rotation;
            transform.position = CurrentHeldPosition.position;
            transform.rotation = CurrentHeldPosition.rotation;
        }
        public override void UseItem()
        {

        }
        void OnEnable()
        {
            RequestDeparentServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        void RequestDeparentServerRpc()
        {
            DeparentClientRpc();
        }

        [ClientRpc(RequireOwnership = false)]
        void DeparentClientRpc()
        {
            if (this.NetworkObject.TryRemoveParent())
            {
                Debug.Log("True removed parent?");
            }
            else
            {
                Debug.Log("False Didnt remove parent?");
            }
        }
    }
}
