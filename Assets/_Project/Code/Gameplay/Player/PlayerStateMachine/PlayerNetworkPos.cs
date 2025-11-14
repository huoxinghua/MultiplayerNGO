using UnityEngine;
using Unity.Netcode;
namespace _Project.Code.Gameplay.Player.PlayerStateMachine
{
    public class PlayerNetworkPos : NetworkBehaviour
    {
        public NetworkVariable<Vector3> ServerPosition = 
            new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);

        private void Update()
        {
            if (IsOwner)
                ServerPosition.Value = transform.position;
        }
    }
}
