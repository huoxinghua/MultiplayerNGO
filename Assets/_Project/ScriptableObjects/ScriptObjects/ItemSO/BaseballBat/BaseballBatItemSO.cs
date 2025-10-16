using UnityEngine;

[CreateAssetMenu(fileName = "BaseballBatItemSO", menuName = "Items/BaseballBatItemSO")]
public class BaseballBatItemSO : BaseItemSO
{
    [field: Header("Baseball Bat Attack")]
    [field: SerializeField] public float Damage { get; private set; }
    [field: SerializeField] public float KnockoutPower { get; private set; }
    [field: SerializeField] public float AttackDistance { get; private set; }
    [field: SerializeField] public float AttackRadius { get; private set; }

}
