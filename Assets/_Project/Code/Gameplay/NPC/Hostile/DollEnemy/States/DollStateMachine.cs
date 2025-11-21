using System;
using System.Collections.Generic;
using _Project.Code.Art.AnimationScripts.Animations;
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
            TransitionTo(StateEnum.LookedAtState);
        }

        public void HandleLookAway()
        {
            //always hunting when looking away
            TransitionTo(StateEnum.HuntingState);
        }

        public void HandleNoValidPlayers()
        {
            TransitionTo(StateEnum.WanderState);
        }
        public void HandleInKillDistance(GameObject playerToKill)
        {
            //only kill in hunting state. 
            if (CurrentState != StateDictionary[StateEnum.HuntingState]) return;
            RequestKillPlayerServerRpc(new NetworkObjectReference(playerToKill.GetComponent<NetworkObject>()));
        }

        public void HandleHuntingTimerDone()
        {
            //if enemy is looked at when timer is done, it will go to hunting state anyways. This prevents enemy moving when it should not
            if(CurrentState == StateDictionary[StateEnum.LookedAtState]) return;
            TransitionTo(StateEnum.HuntingState);
            //set hunter player
        }

        //Should it be RPC?
        public void SetHuntedPlayer(Transform player)
        {
            CurrentPlayerToHunt = player;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestKillPlayerServerRpc(NetworkObjectReference playerObjRef)
        {
            if (playerObjRef.TryGet(out NetworkObject playerNetObj))
            {
                //call a bajillion damage on player
            }
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