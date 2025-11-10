using Steamworks;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.UI
{
    public class PlayeNameTag : NetworkBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        private NetworkVariable<FixedString32Bytes> playerName =
            new NetworkVariable<FixedString32Bytes>(
                default,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner
            );

        public override void OnNetworkSpawn()
        {
            playerName.OnValueChanged += (oldName, newName) =>
            {
                nameText.text = newName.ToString();
            };

            if (IsOwner)
            {
         
                playerName.Value = SteamFriends.GetPersonaName();
            }
            else
            {
                nameText.text = playerName.Value.ToString();
            }
        }
    }
}
