using UnityEngine;

public abstract class BaseEnemySO : ScriptableObject
{
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float maxConsciousness;
    [SerializeField] private float maxHealth;
    [SerializeField] private float damage;
    [SerializeField] private float stoppingDist;

    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float MaxConsciousness => maxConsciousness;
    public float MaxHealth => maxHealth;
    public float Damage => damage;
    public float StoppingDist => stoppingDist;
}
