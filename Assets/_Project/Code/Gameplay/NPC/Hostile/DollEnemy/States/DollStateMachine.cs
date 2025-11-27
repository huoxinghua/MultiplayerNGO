using System;
using System.Collections.Generic;
using _Project.Code.Art.AnimationScripts.Animations;
using _Project.Code.Gameplay.Player.PlayerHealth;
using _Project.Code.Utilities.StateMachine;
using _Project.Code.Utilities.Utility;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace _Project.Code.Gameplay.NPC.Hostile.DollEnemy.States
{
    public class DollStateMachine : BaseStateController
    {
        #region State Variables

        protected DollBaseState CurrentState;
        /*public DollBaseState WanderState { get; private set; }
        public DollBaseState HuntingState { get; private set; }
        public DollBaseState LookedAtState { get; private set; }*/

        #endregion

        public Dictionary<StateEnum,DollBaseState> StateDictionary = new  Dictionary<StateEnum, DollBaseState>();
        [field: SerializeField] public DollSO DollSO { get; private set; }

        #region Component References

        [field: SerializeField] public NavMeshAgent Agent { get; private set; }
        [field: SerializeField] public DollAnimation Animator { get; private set; }

        #endregion

        //this, or a networkobj reference? both?
        public Transform CurrentPlayerToHunt {get; private set;}

        #region NetworkVariables
        protected NetworkVariable<StateEnum> CurrentStateAsEnum = new NetworkVariable<StateEnum>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        // ^ syncing the currentState itself ^
        
        
        #endregion

        #region Setup

        private void Awake()
        {
            StateDictionary.Add(StateEnum.WanderState, new DollWanderState(this, StateEnum.WanderState));
            StateDictionary.Add(StateEnum.HuntingState,  new DollHuntingState(this, StateEnum.HuntingState));
            StateDictionary.Add(StateEnum.LookedAtState, new DollLookedAtState(this, StateEnum.LookedAtState));
            Agent = GetComponent<NavMeshAgent>();
            Animator = GetComponentInChildren<DollAnimation>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                TransitionTo(StateEnum.WanderState);
            }

            CurrentStateAsEnum.OnValueChanged += HandleCurrentStateAsIntChange;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        }

        #endregion

        #region Perception Calls

        public void HandleLookedAt()
        {
            if(!IsServer) return;
          //  Debug.Log("HandleLookedAt");
            CurrentState.StateLookedAt();
        }

        public void HandleLookAway()
        {
            if(!IsServer) return;
            //always hunting when looking away
          //  Debug.Log("HandleLookAway");

            CurrentState.StateLookedAway();
        }

        public void HandleNoValidPlayers()
        {
            if(!IsServer) return;
            CurrentState.StateNoValidPlayer();
        }
        public void HandleInKillDistance(GameObject playerToKill)
        {
            if(!IsServer) return;
            //only kill in hunting state. 
            CurrentState.StateAttemptKill();
        }

        public void HandleHuntingTimerDone()
        {
            if(!IsServer) return;
            //if enemy is looked at when timer is done, it will go to hunting state anyways. This prevents enemy moving when it should not
           CurrentState.StateHuntTimerComplete();
            //set hunter player
        }

        //Should it be RPC?
        public void SetHuntedPlayer(Transform player)
        {
            CurrentPlayerToHunt = player;
        }
        
        public void RequestKill(GameObject playerObj)
        {
            if(!IsServer) return;
            Debug.Log("Trying to kill");
            if (playerObj == null)
            {
                Debug.Log("Failed playerObj is null");
                return;
            }

            var health = playerObj.GetComponent<IPlayerHealth>();
            if (health == null)
            {
                Debug.Log("Failed health is null");
                return;
            }
            //magically big number :)
            health.TakeDamage(100000f);
        }

        #endregion

        #region NetworkVariable Logic



        [ServerRpc(RequireOwnership = false)]
        public void RequestChangeNetStateServerRpc(StateEnum netState)
        {
            if(CurrentStateAsEnum.Value == netState ) return;
            CurrentStateAsEnum.Value = netState;
        }

        private void HandleCurrentStateAsIntChange(StateEnum oldState, StateEnum newState)
        {
            if (!IsServer)
            {
                CurrentState = StateDictionary[newState];
            }
        }
        #endregion
        
    
        #region StateMachine Basics

        private void Update()
        {
            if (!IsServer) return;
            CurrentState.StateUpdate();
        }

        private void FixedUpdate()
        {
            if (!IsServer) return;
            CurrentState.StateFixedUpdate();
        }

        public virtual void TransitionTo(StateEnum newState)
        {
            if (!IsServer) return;
            if (StateDictionary[newState] == CurrentState) return;
            CurrentState?.OnExit();
            CurrentState = StateDictionary[newState];
            RequestChangeNetStateServerRpc(newState);
            Debug.Log($"Current state is {CurrentState}");
            CurrentState.OnEnter();
        }

        #endregion

        public StateEnum GetCurrentState()
        {
            return CurrentStateAsEnum.Value;
        }
    }
    public enum StateEnum
    {
        WanderState = 0,
        HuntingState = 1,
        LookedAtState = 2,
    }
    
}