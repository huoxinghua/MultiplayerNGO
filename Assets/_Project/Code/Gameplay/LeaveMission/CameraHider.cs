using System;
using _Project.Code.Gameplay.LeaveMission;
using _Project.Code.Utilities.EventBus;
using UnityEngine;

public class CameraHider : MonoBehaviour
{
   [SerializeField] private GameObject _playerCam;
   private void Awake()
   {
      EventBus.Instance.Subscribe<LeavingMissionEvent>(this,HandleLeaveMission );
   }

   private void HandleLeaveMission(LeavingMissionEvent e)
   {
      _playerCam.SetActive(false);
   }
}
