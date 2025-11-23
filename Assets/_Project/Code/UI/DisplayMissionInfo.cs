using _Project.Code.Network.GameManagers;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Code.UI
{
    public class DisplayMissionInfo : MonoBehaviour
    {
        public GameObject DisplayInfo;
        [SerializeField] private Button _missionButton;
        private void Start()
        {
            DisplayInfo.SetActive(false);
            _missionButton = GetComponent<Button>();
            if (_missionButton != null)
            {
                _missionButton.onClick.AddListener(OnMissionClicked);
            }

        }
        public void Display()
        {
            DisplayInfo.SetActive(true);
        }
        public void DontDisplay()
        {
            DisplayInfo.SetActive(false);
        }
      
      

        private void OnMissionClicked()
        {
            GameFlowManager.Instance. StartMission();
        }
    }
}
