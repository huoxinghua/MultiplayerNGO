using System;
using _Project.Code.Art.AnimationScripts.Animations;
using _Project.Code.Gameplay.Player;
using _Project.Code.Utilities.StateMachine;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

namespace _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute
{
    public class BruteStateMachine : BaseStateController
    {
        protected BruteBaseState CurrentState;
        protected BruteBaseState StateBeforeAttack;
        public BruteIdleState IdleState { get; private set; }
        public BruteWanderState WanderState { get; private set; }
        public BruteHurtIdleState BruteHurtIdleState { get; private set; }
        public BruteHurtWander BruteHurtWander { get; private set; }
        public BruteAlertState BruteAlertState { get; private set; }
        public BruteChaseState BruteChaseState { get; private set; }
        public BruteAttackState BruteAttackState { get; private set; }
        public BruteHeardPlayerState BruteHeardPlayerState { get; private set; }
        public BruteDeadState BruteDeadState { get; private set; }
        public BruteHitState BruteHitState { get; private set; }
        public BruteAnimation Animator { get; private set; }
        public NavMeshAgent agent { get; private set; }
        [SerializeField] public BruteSO BruteSO;
        public Transform HeartPosition { get; private set; }
        [SerializeField] private GameObject _heartPrefab;
        private GameObject _spawnedHeart;
        public GameObject LastHeardPlayer { get; private set; }
        public GameObject PlayerToAttack { get; private set; }
        public int TimesAlerted = 0;
     
        // Network sync for player target
        private readonly NetworkVariable<NetworkObjectReference> _playerTargetRef = new NetworkVariable<NetworkObjectReference>();
       

        public void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            Animator = GetComponentInChildren<BruteAnimation>();
            //HandleHeartSpawn();
            IdleState = new BruteIdleState(this);
            WanderState = new BruteWanderState(this);
            BruteHurtIdleState = new BruteHurtIdleState(this);
            BruteHurtWander = new BruteHurtWander(this);
            BruteAlertState = new BruteAlertState(this);
            BruteChaseState = new BruteChaseState(this);
            BruteAttackState = new BruteAttackState(this);
            BruteHeardPlayerState = new BruteHeardPlayerState(this);
            BruteDeadState = new BruteDeadState(this);
            BruteHitState = new BruteHitState(this);
        }

        public void HandleHeartSpawn()
        {
            if(!IsServer)return;
            _spawnedHeart = Instantiate(_heartPrefab, transform);
            var netObj = _spawnedHeart.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsSpawned)
            {
                netObj.Spawn();
            }
            _spawnedHeart.GetComponent<BruteHeart>()?.SetStateController(this);
            _spawnedHeart.transform.SetParent(null);
            HeartPosition = transform;
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _playerTargetRef.OnValueChanged += OnPlayerTargetRefChanged;
            if (!IsServer)
            {
                TryResolvePlayerTarget(_playerTargetRef.Value);
            }
            if (IsServer)
            {
                HandleHeartSpawn();//move this called from awake 
                transform.parent = null;
                TransitionTo(WanderState);
            }
        }
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _playerTargetRef.OnValueChanged -= OnPlayerTargetRefChanged;
        }
        private void OnPlayerTargetRefChanged(NetworkObjectReference previous, NetworkObjectReference current)
        {
            if (!IsServer)
            {
                TryResolvePlayerTarget(current);
            }
        }
        
        private void TryResolvePlayerTarget(NetworkObjectReference playerRef)
        {
            if (playerRef.TryGet(out NetworkObject netObj))
            {
                PlayerToAttack = netObj.gameObject;
                Debug.Log($"[Client] Resolved PlayerToAttack -> {PlayerToAttack.name}");
                return;
            }
        }
       
        void Update()
        {
            //Debug.Log("Current State: " + CurrentState);
            if (!IsServer) return;
            CurrentState?.StateUpdate();
        }
        void FixedUpdate()
        {
            if (!IsServer) return;
            CurrentState?.StateFixedUpdate();
        }
        public void OnHearPlayer(GameObject playerObj)
        {

            if (playerObj == null){ Debug.Log("LeavingEarly"); return; }
            //  Debug.Log(playerObj.name);
            LastHeardPlayer = playerObj;
            if (Vector3.Distance(playerObj.transform.position,transform.position) <= BruteSO.InstantAggroDistance)
            {
                if(CurrentState == BruteChaseState)
                {
                    CurrentState.OnHearPlayer();
                }
                else
                {
                    TransitionTo(BruteChaseState);
                }
                return;
            }
            else
            {
                CurrentState?.OnHearPlayer();
            }
        }
        public void HandleDefendHeart(GameObject attackingPlayer)
        {
            LastHeardPlayer = attackingPlayer;
            TransitionTo(BruteChaseState);
        }
        /*public void OnAttack(GameObject playerToAttack) 
        {
            StateBeforeAttack = CurrentState;
            PlayerToAttack = playerToAttack;
            TransitionTo(BruteAttackState);
        }*/
        public void OnAttack(GameObject playerToAttack)
        {
            StateBeforeAttack = CurrentState;
            if (!IsServer)
            {
                var netObj = playerToAttack.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    RequestAttackServerRpc(netObj);
                }
                return;
            }
            ExecuteAttack(playerToAttack);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void RequestAttackServerRpc(NetworkObjectReference playerRef)
        {
            if (playerRef.TryGet(out NetworkObject playerObj))
            {
                ExecuteAttack(playerObj.gameObject);
            }
        }
        
        private void ExecuteAttack(GameObject playerToAttack)
        {
            PlayerToAttack = playerToAttack;
    
            var netObj = playerToAttack.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                _playerTargetRef.Value = netObj;
                Debug.Log($"[Server] Brute set target to {playerToAttack.name}");
            }
            else
            {
                Debug.LogWarning("[Server] Player target missing NetworkObject!");
            }

            TransitionTo(BruteAttackState);
        }

        public void OnDeath()
        {

        
        }

        public void TempAnimMove()
        {
            if (!IsServer) return;
            CurrentState?.OnStateAnimatorMove();
        }

        public void OnHeartDestroyed()
        {
            TransitionTo(BruteHurtIdleState);
        }
        public void OnAttackEnd()
        {
            TransitionTo(StateBeforeAttack);
        }
        public void OnAttackConnects()
        {
            if (PlayerToAttack == null)
            {
                Debug.LogError("[Brute] PlayerToAttack is null in OnAttackConnects!");
                return;
            }

            var health = PlayerToAttack.GetComponent<IPlayerHealth>();
            if (health == null)
            {
                Debug.LogError("[Brute] Target has no IPlayerHealth component!");
                return;
            }
            if(Vector3.Distance(transform.position,PlayerToAttack.transform.position) <= BruteSO.AttackDistance)
            {
                PlayerToAttack.GetComponent<IPlayerHealth>().TakeDamage(BruteSO.Damage);
            }
        }
        public virtual void TransitionTo(BruteBaseState newState)
        {
            if (!IsServer) return;
            if (newState == CurrentState) return;
            CurrentState?.OnExit();
            CurrentState = newState;
            CurrentState.OnEnter();
        }
    }
}