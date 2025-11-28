using System.Collections;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Gameplay.Player.PlayerHealth;
using _Project.Code.Network.GameManagers;
using _Project.Code.Network.ProximityChat.Voice;
using _Project.Code.Utilities.EventBus;
using _Project.Code.Utilities.Singletons;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.Player.PlayerStateMachine
{
    public class PlayerDeadState : PlayerBaseState
    {
        public PlayerDeadState(PlayerStateMachine stateController) : base(stateController) { }
        private const float DespawnDelaySeconds = 2f;
        private Coroutine _despawnRoutine;

        public override void OnEnter()
        {
           
            var netObject = stateController.GetComponent<NetworkObject>();
            bool isOwner = netObject != null && netObject.IsOwner;

            stateController.VerticalVelocity = Vector3.zero;

            if (stateController.CharacterController != null)
                stateController.CharacterController.enabled = false;
            var cam = stateController.gameObject.GetComponentInChildren<Camera>();
            cam.enabled = false;
            CurrentPlayers.Instance?.RemovePlayer(stateController.gameObject);
            PlayerStateMachine.AllPlayers.Remove(stateController);
            
            if (netObject != null && netObject.IsSpawned)
                netObject.Despawn(true);
            bool isServer = NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;


            if (!isOwner)
                return;
            
            var recorder = stateController.GetComponentInChildren<VoiceRecorder>();
            if (recorder != null)
            {
                recorder.StopRecording();
                recorder.enabled = false;
            }

            EventBus.Instance?.Publish(new PlayerDiedEvent { deadPlayer = stateController.gameObject });
        }

        public override void OnExit()
        {
            if (_despawnRoutine != null)
            {
                stateController.StopCoroutine(_despawnRoutine);
                _despawnRoutine = null;
            }

            if (stateController.CharacterController != null)
                stateController.CharacterController.enabled = true;

            if (stateController.InputManager != null)
            {
                stateController.InputManager.enabled = true;
                stateController.InputManager.SwitchToPlayerMode();
            }

            var spectatorInput = stateController.GetComponent<PlayerInputManagerSpectator>();
            if (spectatorInput != null)
                spectatorInput.enabled = false;
        }

        public override void StateFixedUpdate() { }

        public override void StateUpdate() { }

        #region Input Blocking
        public override void OnUseInput() { }
        public override void OnSecondaryUseInput(bool isPressed) { }
        public override void OnDropItemInput() { }
        public override void OnInteractInput() { }
        public override void OnNumPressedInput(int slot) { }
        public override void OnChangeWeaponInput() { }
        #endregion
        
    }
    public class PlayerDiedEvent : IEvent
    {
        public GameObject deadPlayer;
    }
}
