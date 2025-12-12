using _Project.Code.Utilities.Audio;
using _Project.Code.Utilities.EventBus;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Tranquil.Beetle
{
    public class BeetleAnimationEventSystem : MonoBehaviour
    {
        public void OnFootStep()
        {
            EventBus.Instance.PublishGameplayEvent(new BeetleFootstepsEvent{EventID = EventIDs.EnemyBeetleFootsteps, EventPosition = transform.position});
        }
    }
}
