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

        // [field: SerializeField] public DollSO DollSO { get; private set; }

        #region Component References

        [field: SerializeField] public NavMeshAgent Agent { get; private set; }
        [field: SerializeField] public DollAnimation Animator { get; private set; }

        #endregion

        //this, or a networkobj reference? both?
        private Transform _currentPlayerToHunt;

        #region NetworkVariables

        //These two?
        protected NetworkVariable<bool> IsLookedAt = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        protected NetworkVariable<bool> IsHunting = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        //or...

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

        public void HandleInKillDistance(GameObject playerToKill)
        {
            //only kill in hunting state. 
            if (CurrentState != HuntingState) return;
            RequestKillPlayerServerRpc(new NetworkObjectReference(playerToKill.GetComponent<NetworkObject>()));
        }

        public void HandeHuntingTimerDone()
        {
            //if enemy is looked at when timer is done, it will go to hunting state anyways. This prevents enemy moving when it should not
            if(CurrentState == LookedAtState) return;
            TransitionTo(HuntingState);
            //set hunter player
        }

        //Should it be RPC?
        public void SetHuntedPlayer(Transform player)
        {
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

        #region IsLookedAt bool
        
        [ServerRpc(RequireOwnership = false)]
        public void RequestChangeIsLookedAtServerRpc(bool isLookedAt)
        {
            if (isLookedAt == IsLookedAt.Value) return;
            IsLookedAt.Value = isLookedAt;
        }

        private void HandleLookedAtChange(bool oldState, bool newState)
        {
            
        }
        #endregion

        #region IsHunting bool
        [ServerRpc(RequireOwnership = false)]
        public void RequestChangeIsHuntingServerRpc(bool isHunting)
        {
            if (isHunting == IsHunting.Value) return;
            IsHunting.Value = isHunting;
        }

        private void HandleHuntingStateChange(bool oldState, bool newState)
        {
            
        }
        
        #endregion

        #region CurrentStateAsInt int

        [ServerRpc(RequireOwnership = false)]
        public void RequestChangeNetStateServerRpc(DollStatesAsInt netStatesAsInt)
        {
            if(CurrentStateAsInt.Value == (int)netStatesAsInt)  return;
            CurrentStateAsInt.Value = (int)netStatesAsInt;
        }

        private void HandleCurrentStateAsIntChange(int oldState, int newState)
        {
            
        }
        #endregion
        #endregion
        
        
        //Is this okay? Only if I should sync state... IDK
        public DollBaseState FromEnum(DollStatesAsInt state)
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
            CurrentState.OnEnter();
        }

        #endregion
    }
    public enum DollStatesAsInt
    {
        WanderState = 0,
        HuntingState = 1,
        LookedAtState = 2,
    }
    
}