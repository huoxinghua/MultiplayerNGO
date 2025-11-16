using _Project.Code.Network.ProximityChat.Voice;
using UnityEngine;

namespace _Project.Code.Gameplay.Player.PlayerStateMachine
{
    public class PlayerDeadState : PlayerBaseState
    {
        public PlayerDeadState(PlayerStateMachine stateController) : base(stateController)
        {
            
        }
        public override void OnEnter()
        {
            Debug.Log("PlayerDeadState OnEnter");
            var recorder = stateController.gameObject.GetComponentInChildren<VoiceRecorder>();
            if (recorder != null)
            {
                recorder.StopRecording();
                recorder.enabled = false;
            }
            
            // 2. Trigger spectator mode
            // remove the player in the list
       
            
        }
        public override void OnExit()
        {
     
        }

        public override void StateFixedUpdate()
        {

        }

        public override void StateUpdate()
        {
  
          
        }
    }
}
