using System.Collections.Generic;
using _Project.Code.Gameplay.Interfaces;
using _Project.Code.Gameplay.NPC.Tranquil.Beetle.BeetleRefactor;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Tranquil.Beetle
{
    public class BeetleHealth : NetworkBehaviour,IHitable
    {
        //add players who attacked to list
        [SerializeField] private BeetleSO _beetleSO;
        public BeetleStateMachine StateMachine { get; private set; }
        public List<GameObject> HostilePlayers = new List<GameObject>();
        private float _maxHealth;
        private float _currentHealth;
        private float _maxConsciousness;
        private float _currentConsciousness;
       
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            StateMachine = GetComponent<BeetleStateMachine>();  
            _maxHealth = _beetleSO.MaxHealth;
            _currentHealth = _maxHealth;
            _maxConsciousness = _beetleSO.MaxConsciousness;
            _currentConsciousness = _maxConsciousness;
        }

        public bool IsPlayerHostile(GameObject playerToCheck)
        {
            bool isHostile = false;
            foreach(var hostilePlayer  in HostilePlayers)
            {
                if(playerToCheck == hostilePlayer)
                {
                    isHostile = true;
                }
            }
            return isHostile;
        }
        public void ChangeHealth(float healthChange)
        {
            if(!IsServer)return;
            _currentHealth += healthChange;
            if (_currentHealth < 0)
            {
                OnDeath();
            }
        }
        public void OnKnockout()
        {
            StateMachine.HandleKnockedOut();
        }
        public void OnDeath()
        {
            StateMachine.HandleDeath();
        }
       
        public void ChangeConsciousness(float consciousnessChange)
        {
            _currentConsciousness += consciousnessChange;
            if(_currentConsciousness < 0)
            {
                OnKnockout();
            }
        }
        public void OnHit(GameObject attacker, float damage, float knockoutPower)
        {
            if (!IsServer)
            {
                var attackerNetObj = attacker.GetComponent<NetworkObject>();
                if (attackerNetObj != null)
                {
                    OnHitServerRpc(attackerNetObj, damage, knockoutPower);
                }
                return;
            }

            ApplyHit(attacker, damage, knockoutPower);
        }

        public void ApplyHit(GameObject attacker, float damage, float knockoutPower)
        {
          
            if (attacker.layer == 6)
            {
                bool isInList = false;
                foreach(var player in HostilePlayers)
                {
                    if (player == attacker)
                    {
                        isInList = true;
                    }
                }
                if (!isInList) HostilePlayers.Add(attacker);
            }
            StateMachine.HandleHitByPlayer(attacker);
            ChangeHealth(-damage);
            ChangeConsciousness(-knockoutPower);
        }
        [ServerRpc(RequireOwnership = false)]
        private void OnHitServerRpc(NetworkObjectReference attackerRef, float damage, float knockoutPower)
        {
            if (!attackerRef.TryGet(out NetworkObject attackerObj)) return;
            ApplyHit(attackerObj.gameObject, damage, knockoutPower);
        }
    }
}
