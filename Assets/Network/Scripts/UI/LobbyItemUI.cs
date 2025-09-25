using Project.Network.SteamWork;
using Steamworks;
using System.Net.NetworkInformation;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text pingText;
    [SerializeField] private TMP_Text serverNameText;
    public CSteamID LobbyId { get; private set; }
    public void SetData(string lobbyName, int ping, CSteamID lobbyId)
    {
        serverNameText.text = lobbyName;
        pingText.text = ping >= 0 ? $"Ping: {ping} ms" : ping.ToString();
        LobbyId = lobbyId;
    }
    public void SetPing(int ping)
    {
        if (pingText !=null)
        {
            pingText.text = ping >= 0 ? $"Ping: {ping} ms" : ping.ToString();
        }

    }
  
}
