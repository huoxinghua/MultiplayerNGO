using UnityEngine;

namespace _Project.Code.UI
{
    public class DisplayMissionInfo : MonoBehaviour
    {
        public GameObject DisplayInfo;

        private void Start()
        {
            DisplayInfo.SetActive(false);
        }
        public void Display()
        {
            DisplayInfo.SetActive(true);
        }
        public void DontDisplay()
        {
            DisplayInfo.SetActive(false);
        }
    }
}
