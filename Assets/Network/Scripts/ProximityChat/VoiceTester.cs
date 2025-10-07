using UnityEngine;

namespace Project.Network.Voice
{
    public class MicTest : MonoBehaviour
    {
        void Start()
        {
            foreach (var device in Microphone.devices)
            {
               // Debug.Log("Mic device: " + device);
            }
        }
    }
}

