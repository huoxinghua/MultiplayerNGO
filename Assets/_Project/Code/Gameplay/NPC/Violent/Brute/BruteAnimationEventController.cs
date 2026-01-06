using _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute;
using _Project.Code.Utilities.Audio;
using _Project.Code.Utilities.EventBus;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute
{
    public class BruteAnimationEventController : MonoBehaviour
    {
        [SerializeField] private BruteStateMachine _stateMachine;
        public void OnFootStep()
        {
            EventBus.Instance.PublishGameplayEvent(new BruteFootstepsEvent{EventID = EventIDs.EnemyBruteFootsteps, EventPosition = transform.position});
        }
        public void OnAttackNoise()
        {
            EventBus.Instance.PublishGameplayEvent(new BruteAttackEvent{EventID = EventIDs.EnemyBruteAttack, EventPosition = transform.position});
        }
        public void OnAttackConnect()
        {
            _stateMachine.OnAttackConnects();   
        }
        public void OnAttackEnd()
        {
            _stateMachine.OnAttackEnd();
        }
    }
}
