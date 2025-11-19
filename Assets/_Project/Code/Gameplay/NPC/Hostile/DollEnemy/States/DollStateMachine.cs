using System;
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
        public DollBaseState WanderState { get; private set; }
        public DollBaseState HuntingState { get; private set; }
        public DollBaseState LookedAtState { get; private set; }

        #endregion

        [field: SerializeField] public DollSO DollSO { get; private set; }

        #region Component References

        [field: SerializeField] public NavMeshAgent Agent { get; private set; }
        [field: SerializeField] public DollAnimation Animator { get; private set; }

        #endregion

        //this, or a networkobj reference? both?
        public Transform CurrentPlayerToHunt {get; private set;}

        #region NetworkVariables
        protected NetworkVariable<int> CurrentStateAsInt = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        // ^ syncing the currentState itself ^
        
        
        #endregion

        #region Setup

        private void Awake()
        {
            WanderState = new DollWanderState(this);
            HuntingState = new DollHuntingState(this);
            LookedAtState = new DollLookedAtState(this);

            Agent = GetComponent<NavMeshAgent>();
            Animator = GetComponentInChildren<DollAnimation>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                TransitionTo(WanderState);
            }

            CurrentStateAsInt.OnValueChanged += HandleCurrentStateAsIntChange;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        }

        #endregion

        #region Perception Calls

        public void HandleLookedAt()
        {
            TransitionTo(LookedAtState);
        }

        public void HandleLookAway()
        {
            //always hunting when looking away
            TransitionTo(HuntingState);
        }

        public void HandleNoValidPlayers()
        {
            TransitionTo(WanderState);
        }
        public void HandleInKillDistance(GameObject playerToKill)
        {
            //only kill in hunting state. 
            if (CurrentState != HuntingState) return;
            RequestKillPlayerServerRpc(new NetworkObjectReference(playerToKill.GetComponent<NetworkObject>()));
        }

        public void HandleHuntingTimerDone()
        {
            //if enemy is looked at when timer is done, it will go to hunting state anyways. This prevents enemy moving when it should not
            if(CurrentState == LookedAtState) return;
            TransitionTo(HuntingState);
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
        public void RequestChangeNetStateServerRpc(DollStatesAsInt netStatesAsInt)
        {
            if(CurrentStateAsInt.Value == (int)netStatesAsInt)  return;
            CurrentStateAsInt.Value = (int)netStatesAsInt;
        }

        private void HandleCurrentStateAsIntChange(int oldState, int newState)
        {
            if (!IsServer)
            {
                CurrentState = FromStateEnum((DollStatesAsInt)newState);
            }
        }
        #endregion
        
        #region States As Enum Util
        public DollBaseState FromStateEnum(DollStatesAsInt state)
        {
            switch (state)
            {
                case DollStatesAsInt.WanderState:
                    return WanderState;

                case DollStatesAsInt.HuntingState:
                    return HuntingState;

                case DollStatesAsInt.LookedAtState:
                    return LookedAtState;

                default:
                    Debug.Log("Unknown state");
                    return null;
                    break;
            }
        }

        public DollStatesAsInt ToStateEnum(DollBaseState state)
        {
            if (state == WanderState)
            {
                return DollStatesAsInt.WanderState;
            }
            else if (state == HuntingState)
            {
                return DollStatesAsInt.HuntingState;
            }
            else if (state == LookedAtState)
            {
                return DollStatesAsInt.LookedAtState;
            }
            else
            {
                return default;  
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

        public virtual void TransitionTo(DollBaseState newState)
        {
            if (!IsServer) return;
            if (newState == CurrentState) return;
            CurrentState?.OnExit();
            CurrentState = newState;
            RequestChangeNetStateServerRpc(ToStateEnum(newState));
            CurrentState.OnEnter();
        }

        #endregion

        public DollBaseState GetCurrentState()
        {
            return CurrentState;
        }
    }
    public enum DollStatesAsInt
    {
        WanderState = 0,
        HuntingState = 1,
        LookedAtState = 2,
    }
    
}