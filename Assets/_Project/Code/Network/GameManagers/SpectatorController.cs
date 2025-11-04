using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Player;
using _Project.Code.Gameplay.Player.PlayerStateMachine;
using _Project.Code.Utilities.Singletons;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

namespace _Project.Code.Network.GameManagers
{
    public class SpectatorController : Singleton<SpectatorController>
    {
        [SerializeField] private CinemachineCamera _spectatorCam;
        private readonly List<Transform> _aliveHeads = new List<Transform>();
        [SerializeField] private Camera mainCam;
        private int _currentIndex = 0;


        public void EnterSpectatorMode()
        {
          
            StartCoroutine(DelayedRefresh());
         
          //  RefreshAliveList();
   

            if (_aliveHeads.Count == 0)
            {
                Debug.Log("No alive players to spectate.");
                FindObjectOfType<NetworkSessionReset>()?.ReturnToMainMenu();
                return;
            }
            mainCam.enabled = true;

            _spectatorCam.Priority = 20;
            SetTarget(_aliveHeads[0]);
        }

        private void Update()
        {
            if (_spectatorCam.Priority < 20) return;

            if (Input.GetKeyDown(KeyCode.N)) Next();
            if (Input.GetKeyDown(KeyCode.B)) Prev();
        }

        private IEnumerator DelayedRefresh()
        {
            const int maxTries = 5;
            const float delayBetweenTries = 0.3f;

            for (int i = 0; i < maxTries; i++)
            {
                RefreshAliveList();
                if (_aliveHeads.Count > 0) break;

                Debug.Log($"[Spectator] Try {i + 1}: No alive players yet...");
                yield return new WaitForSeconds(delayBetweenTries);
            }

            if (_aliveHeads.Count == 0)
            {
                Debug.LogWarning("[Spectator] Still no alive players after retries.");
                yield break;
            }
        }

        private void RefreshAliveList()
        {
            _aliveHeads.Clear();

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var playerObj = client.PlayerObject;
                if (playerObj == null)
                {
                    Debug.LogWarning($"[Spectator] PlayerObject is null for client {client.ClientId}");
                    continue;
                }

     
                if (client.ClientId == NetworkManager.Singleton.LocalClientId)
                    continue;

                var health = playerObj.GetComponent<PlayerHealth>();
                if (health != null && health.IsDead)
                    continue;

                var head = playerObj.transform;
                if (head != null)
                {
                    _aliveHeads.Add(head);
                    Debug.Log($"[Spectator] Added alive player head: {head.name}");
                }
            }

            Debug.Log($"[Spectator] Found {_aliveHeads.Count} alive players.");
        }


        private void SetTarget(Transform t)
        {
           Debug.Log("setTarget"+t);
            _spectatorCam.Follow = t;
            _spectatorCam.LookAt = t;
            if (!_spectatorCam.TryGetComponent(out CinemachineFollow follow))
            {
                follow = _spectatorCam.gameObject.AddComponent<CinemachineFollow>();
                Debug.Log("[Spectator] Added CinemachineFollow (Position Control = Follow)");
            }

            if (!_spectatorCam.TryGetComponent(out CinemachineRotateWithFollowTarget rotate))
            {
                rotate = _spectatorCam.gameObject.AddComponent<CinemachineRotateWithFollowTarget>();
                Debug.Log("[Spectator] Added CinemachineRotateWithFollowTarget (Rotation Control = Rotate With Follow Target)");
            }
            Debug.Log($"Spectating: {t.name}");
          
            follow.FollowOffset = new Vector3(0f, 1f, -2f);
       
        }

        private void Next()
        {
            if (_aliveHeads.Count == 0) return;
            _currentIndex = (_currentIndex + 1) % _aliveHeads.Count;
            SetTarget(_aliveHeads[_currentIndex]);
        }

        private void Prev()
        {
            if (_aliveHeads.Count == 0) return;
            _currentIndex = (_currentIndex - 1 + _aliveHeads.Count) % _aliveHeads.Count;
            SetTarget(_aliveHeads[_currentIndex]);
        }
    }
}