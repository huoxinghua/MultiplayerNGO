using UnityEngine;

namespace Project.Network.UI
{
    public class HostUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject hostOption;
        private bool isOpen = false;

        private void Start()
        {
            HideHostOption();
        }
        public void ShowHostOption()
        {
            if (!isOpen)
            {
                hostOption.SetActive(true);
            }
        }
        public void HideHostOption()
        {

            hostOption.SetActive(false);
        }

    }

}


