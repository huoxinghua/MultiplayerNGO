using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Network.UI
{
    public class GameTimer : NetworkBehaviour
    {
        private NetworkVariable<double> _startTime = new NetworkVariable<double>(0);
        [SerializeField] private TMP_Text timerText;
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                _startTime.Value = NetworkManager.ServerTime.Time;
            }
        }
        public double GetElapsedTime()
        {
            return NetworkManager.ServerTime.Time - _startTime.Value;
        }
        private void Update()
        {
            double elapsed = GetElapsedTime();
            int minutes = Mathf.FloorToInt((float)(elapsed / 60f));
            int seconds = Mathf.FloorToInt((float)(elapsed % 60f));
            timerText.text = $"{minutes:00}:{seconds:00}";
        }

    }
}

