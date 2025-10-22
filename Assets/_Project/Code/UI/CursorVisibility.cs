using UnityEngine;

namespace _Project.Code.UI
{
    public class CursorVisibility : MonoBehaviour
    {

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                Debug.Log("Working");
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

    }
}
