using _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute
{
    public class BruteAttack : MonoBehaviour
    {
        [SerializeField] private BruteStateMachine _stateMachine;
        [SerializeField] private BruteSO _bruteSO;
        private bool _isOnCooldown;
    
        float attackDistance => _bruteSO.AttackDistance;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

    }
}
