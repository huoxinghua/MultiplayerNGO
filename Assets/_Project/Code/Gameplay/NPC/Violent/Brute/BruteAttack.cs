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
    }
}
