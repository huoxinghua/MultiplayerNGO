using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Hostile.DollEnemy
{
    public class DollSO : BaseEnemySO 
    {
        [field: Header("Doll Timers")]
        [field: SerializeField] public float FirstHuntCooldown { get; private set; }
        [field: SerializeField] public float SubsequentHuntCooldown { get; private set; }
        [field: SerializeField] public float PerceptionFrequency { get; private set; }
        
        [field: Header("Doll Ranges")]
        [field: SerializeField] public float ProximityRange { get; private set; }
        [field: SerializeField] public float KillRange { get; private set; }
        
        [field: Header("Doll Vision")]
        [field: SerializeField] public float DollFOV { get; private set; }
    }
}