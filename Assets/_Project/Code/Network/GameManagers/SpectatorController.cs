using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Player;
using _Project.Code.Utilities.EventBus;
using Unity.Cinemachine;
using _Project.Code.Gameplay.FirstPersonController;
using Unity.Entities;


namespace _Project.Code.Network.GameManagers
{
    public class SpectatorController : Singleton<SpectatorController>
    {
        //[SerializeField] private CinemachineCamera _spectatorCam;
        [SerializeField] private Camera mainCam;
        private readonly List<Transform> _aliveHeads = new List<Transform>();
        Vector2 rawLook;
        private int _currentIndex = 0;
        private PlayerInputManagerSpectator _input;

        [Header("Sensitivity Values")] [SerializeField]
        private float sensitivity = 2;

        [SerializeField] private float smoothing = 1.5f;
        [SerializeField] private float rawLookMultiply = 0.009f;
        Vector2 velocity;
        Vector2 frameVelocity;
        private Transform _currentTarget;
        private float _yaw;
        private float _pitch;
        [SerializeField] private float followDistance = 4f;
        [SerializeField] private float heightOffset = 1.5f;
        private void OnEnable()
        {
            EventBus.Instance.Subscribe<PlayerDiedEvent>(this, EnterSpectatorMode);
        }


        public void EnterSpectatorMode(PlayerDiedEvent playerDiedEvent)
        {
            StartCoroutine(DelayedRefresh(playerDiedEvent));


            //  RefreshAliveList();
           // Debug.Log($"EnterSpectatorMode{playerDiedEvent.deadPlayer}");

            if (_aliveHeads.Count == 0)
            {
                Debug.Log("No alive players to spectate.");
                FindObjectOfType<NetworkSessionReset>()?.ReturnToMainMenu();
                return;
            }

            mainCam.enabled = true;
            Debug.Log($"mainCam{mainCam.name}");
            _input =GetComponent<PlayerInputManagerSpectator>();
            if (_input == null)
            {
                Debug.Log("[Spectator] No PlayerInputManagerSpectator found on dead player.");
                return;
            }
            Debug.Log($"_spectator input found already{_input.name}");
            _input.EnableSpectatorInput();
            _input.enabled = true;
            _input.OnSpectatorLookInput += Look;
            
            if (_aliveHeads.Count > 0)
            {
                SetTarget(_aliveHeads[0]);
            }

            _input.OnNext += Next;

            _input.OnPrev += Prev;
         


            //   _spectatorCam.Priority = 20;
            //SetTarget(_aliveHeads[0]);
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.OnSpectatorLookInput -= Look;
                _input.OnNext -= Next;
                _input.OnPrev -= Prev;
            }
            
        }


        private void Update()
        {
            //  if (_spectatorCam.Priority < 20) return;

            HandleCamera();
        }

        private IEnumerator DelayedRefresh(PlayerDiedEvent playerDiedEvent)
        {
            const int maxTries = 5;
            const float delayBetweenTries = 0.3f;

            for (int i = 0; i < maxTries; i++)
            {
                RefreshAliveList(playerDiedEvent);
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

        private void RefreshAliveList(PlayerDiedEvent playerDiedEvent)
        {
            _aliveHeads.Clear();
           // Debug.Log("how many player alive:" +NetworkManager.Singleton.ConnectedClientsList);
            Debug.Log("Connected clients count = " + NetworkManager.Singleton.ConnectedClientsList.Count);


            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var playerObj = client.PlayerObject;
                if (playerObj == null)
                {
                    Debug.LogWarning($"[Spectator] PlayerObject is null for client {client.ClientId}");
                    continue;
                }


                /*if (client.ClientId == NetworkManager.Singleton.LocalClientId)
                    continue;*/
                if (client.PlayerObject == playerDiedEvent.deadPlayer)
                    continue;
                var health = playerObj.GetComponent<PlayerHealth>();
                if (health != null && health.IsDead)
                    continue;

                var head = playerObj.transform.Find("PlayerCameraRoot");
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
            Debug.Log("setTarget" + t);
            _currentTarget = t;
            _yaw = t.eulerAngles.y;
            _pitch = 10f; 

       
            HandleCamera();
            /*
            _spectatorCam.Follow = t;
            _spectatorCam.LookAt = t;
            if (!_spectatorCam.TryGetComponent(out CinemachineThirdPersonFollow follow))
            {
                follow = _spectatorCam.gameObject.AddComponent<CinemachineThirdPersonFollow>();
                Debug.Log("[Spectator] Added CinemachineThirdPersonFollow (Position Control)");
            }

            if (!_spectatorCam.TryGetComponent(out CinemachineRotateWithFollowTarget rotate))
            {
                rotate = _spectatorCam.gameObject.AddComponent<CinemachineRotateWithFollowTarget>();
                Debug.Log(
                    "[Spectator] Added CinemachineRotateWithFollowTarget (Rotation Control = Rotate With Follow Target)");
            }
            */



            //  follow.FollowOffset = new Vector3(0f, 1f, -2f);

        }
        private void Next()
        {
            if (_aliveHeads.Count == 0) return;
            _currentIndex = (_currentIndex + 1) % _aliveHeads.Count;
            SetTarget(_aliveHeads[_currentIndex]);
            Debug.Log("switch to next spectator came count"+ _aliveHeads.Count+"Index"+_aliveHeads[_currentIndex]);
        }

        private void Prev()
        {
            if (_aliveHeads.Count == 0) return;
            _currentIndex = (_currentIndex - 1 + _aliveHeads.Count) % _aliveHeads.Count;
             SetTarget(_aliveHeads[_currentIndex]);
             Debug.Log("switch to prev spectator came count"+ _aliveHeads.Count+"Index"+_aliveHeads[_currentIndex]);
        }


        private void HandleCamera()
        {
            if (_currentTarget == null) return;

           
            _yaw += rawLook.x * sensitivity;
            _pitch -= rawLook.y * sensitivity;
            _pitch = Mathf.Clamp(_pitch, -80f, 80f);


            Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 offset = rot * new Vector3(0f, heightOffset, -followDistance);

          
            mainCam.transform.position = _currentTarget.position + offset;
            mainCam.transform.rotation = rot;

        }

        private void Look(Vector2 dir)
        {
            rawLook = dir;
        }
    }
}