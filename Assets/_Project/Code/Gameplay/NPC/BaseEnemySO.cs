using UnityEngine;

namespace _Project.Code.Gameplay.NPC
{
    [CreateAssetMenu(fileName = "BaseEnemySO", menuName = "Enemy/BaseEnemySO")]
    public abstract class BaseEnemySO : ScriptableObject
    {
        [field: Header("Base Movement")]
        [field: SerializeField] public float WalkSpeed { get; private set; }
        [field: SerializeField] public float RunSpeed { get; private set; }
        [field: Header("Base Health/Defense")]
        [field: SerializeField] public float MaxConsciousness { get; private set; }
        [field: SerializeField] public float MaxHealth { get; private set; }
        [field: Header("Base Attack")]
        [field: SerializeField] public float Damage { get; private set; }
        [field: Header("Misc")]
        [field: SerializeField] public float StoppingDist { get; private set; }
    }
}
