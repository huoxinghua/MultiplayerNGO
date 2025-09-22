using UnityEngine;
using Unity.Netcode;
using TMPro;

public class GameTimer :NetworkBehaviour
{
    public NetworkVariable<double> StartTime = new NetworkVariable<double>(0);
    [SerializeField] private TMP_Text timerText;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartTime.Value = NetworkManager.ServerTime.Time; 
        }
    }
    public double GetElapsedTime()
    {
        return NetworkManager.ServerTime.Time - StartTime.Value;
    }
    private void Update()
    {
        double elapsed = GetElapsedTime();
        int minutes = Mathf.FloorToInt((float)(elapsed / 60f));
        int seconds = Mathf.FloorToInt((float)(elapsed % 60f));
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

}
