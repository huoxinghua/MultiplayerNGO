using FMODUnity;
using ProximityChat;
using UnityEngine;

namespace Project.Network.Voice
{
    public class VoiceManager : MonoBehaviour
    {
        private VoiceRecorder recorder;

        void Start()
        {
            recorder = GetComponentInChildren<VoiceRecorder>();
            if (recorder == null)
            {
                Debug.LogError("VoiceRecorder not found on Player prefab!");
                return;
            }
            recorder.StartRecording();
            //Debug.Log("Voice recording started!");
        }

        void OnDisable()
        {
            if (recorder != null)
                recorder.StopRecording();
        }
        /*
        void Update()
        {
            if (recorder.IsRecording)
            {
                Debug.Log("Still recording voice...");
            }
        }

        */
    }

}

