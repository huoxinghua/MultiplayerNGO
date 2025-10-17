using System.Collections.Generic;
using UnityEngine;
namespace Project.Network.TestRPC
{
    public class OnScreenLogger : MonoBehaviour
    {
        Queue<string> logs = new Queue<string>();

        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            logs.Enqueue(logString);
            if (logs.Count > 10) logs.Dequeue();
        }

        void OnGUI()
        {
            GUILayout.BeginVertical("box");
            foreach (var log in logs)
            {
                GUILayout.Label(log);
            }
            GUILayout.EndVertical();
        }
    }
}