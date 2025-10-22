using Steamworks;
using TMPro;
using UnityEngine;

namespace Network.Scripts.UI
{
    public class LobbyItemUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _pingText;
        [SerializeField] private TMP_Text _serverNameText;
        public CSteamID LobbyId { get; private set; }
        public void SetData(string lobbyName, int ping, CSteamID lobbyId)
        {
            _serverNameText.text = lobbyName;
            _pingText.text = ping >= 0 ? $"Ping: {ping} ms" : "ping...";
            LobbyId = lobbyId;
        }
        public void SetPing(int ping)
        {
            if (_pingText != null)
            {
                _pingText.text = ping >= 0 ? $"Ping: {ping} ms" : ping.ToString();
            }

        }
    }
}
