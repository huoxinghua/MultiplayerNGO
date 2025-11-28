using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Player;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Gameplay.Player.PlayerHealth;
using _Project.Code.Gameplay.Player.PlayerStateMachine;
using _Project.Code.Utilities.EventBus;
using FMODUnity;

namespace _Project.Code.Network.GameManagers
{
    public class SpectatorController : Singleton<SpectatorController>
    {
        [SerializeField] private Camera mainCam;
        [SerializeField] private GameObject  _voiceNetworkSpectator;
        private readonly List<Transform> _aliveHeads = new List<Transform>();
        Vector2 rawLook;
        private int _currentIndex = 0;
        private PlayerInputManagerSpectator _input;

        [Header("Sensitivity Values")] [SerializeField]
        private float sensitivity = 2;

        private Transform _currentTarget;
        private float _yaw;
        private float _pitch;
        [SerializeField] private float followDistance = 4f;
        [SerializeField] private float heightOffset = 1.5f;
 

        private void Start()
        {
            if (_voiceNetworkSpectator != null)
            {
                _voiceNetworkSpectator.SetActive(false);
            }
            else
            {
                Debug.Log("_voiceNetworkSpectator need set in inspector");
            }
            
        }

        private void OnEnable()
        {
            if (PlayerListManager.Instance != null)
                PlayerListManager.Instance.AlivePlayers.OnListChanged += OnAlivePlayersChanged;
        }

        private void OnDisable()
        {
            /*if (EventBus.Instance != null)
            {
                EventBus.Instance.Subscribe<PlayerDiedEvent>(this, EnterSpectatorMode);
            }*/

            if (_input != null)
            {
                _input.OnSpectatorLookInput -= Look;
                _input.OnNext -= Next;
                _input.OnPrev -= Prev;
            }
            if (PlayerListManager.Instance != null)
                PlayerListManager.Instance.AlivePlayers.OnListChanged -= OnAlivePlayersChanged;
        }
        private void OnAlivePlayersChanged(NetworkListEvent<ulong> changeEvent)
        {
            if (!mainCam.enabled) return; 

            Debug.Log("[Spectator] Alive list changed → Rebuilding");

            StartCoroutine(RebuildDelayed());
        }

        private IEnumerator RebuildDelayed()
        {
            yield return new WaitForEndOfFrame(); 
            yield return null;                      
    
            RebuildAliveHeads();
        }
        private void RebuildAliveHeads()
        {
            _aliveHeads.Clear();

            foreach (ulong clientId in PlayerListManager.Instance.AlivePlayers)
            {
           
                if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
                    continue;

                var playerObj = client.PlayerObject;
                if (playerObj == null)
                    continue;

                var sm = playerObj.GetComponent<PlayerStateMachine>();
                if (sm == null)
                    continue;

               
                _aliveHeads.Add(sm.transform);

                Debug.Log($"[Spectator] Added alive player: {playerObj.name}, clientId={clientId}");
            }
           
            if (_aliveHeads.Count > 0)
                SetTarget(_aliveHeads[0]);
        }
        public void EnterSpectatorMode( )
        {
            if (_input != null)
            {
                _input.OnSpectatorLookInput -= Look;
                _input.OnNext -= Next;
                _input.OnPrev -= Prev;
            }
            _aliveHeads.Clear(); 
            _currentIndex = 0;
            mainCam.enabled = true;
            _input =GetComponent<PlayerInputManagerSpectator>();
            if (_input == null)
            {
                return;
            }
            foreach (ulong clientId in PlayerListManager.Instance.AlivePlayers)
            {
           
                if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
                    continue;

                var playerObj = client.PlayerObject;
                if (playerObj == null)
                    continue;

                var sm = playerObj.GetComponent<PlayerStateMachine>();
                if (sm == null)
                    continue;

               
                _aliveHeads.Add(sm.transform);

                Debug.Log($"[Spectator] Added alive player: {playerObj.name}, clientId={clientId}");
            }
      
            _input.EnableSpectatorInput();
            _input.enabled = true;
            _input.OnSpectatorLookInput += Look;
            
            if (_aliveHeads.Count > 0)
            {
                SetTarget(_aliveHeads[0]);
            }

            _input.OnNext += Next;

            _input.OnPrev += Prev;
            HandleSpectatorVoice();
        }

        private void HandleSpectatorVoice()
        {
            var listener = mainCam. GetComponent<StudioListener>();
            listener.enabled = true;
            if (_voiceNetworkSpectator != null)
            {
                _voiceNetworkSpectator.SetActive(true);
            }
            else
            {
                Debug.Log("_voiceNetworkSpectator null ");
            }
        }

        private void Update()
        {
            HandleCamera();
        }
        

        private void SetTarget(Transform t)
        {
            _currentTarget = t;
            _yaw = t.eulerAngles.y;
            _pitch = 10f; 
            HandleCamera();

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