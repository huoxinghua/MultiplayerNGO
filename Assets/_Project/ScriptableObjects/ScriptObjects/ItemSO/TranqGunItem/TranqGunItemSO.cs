using _Project.ScriptableObjects.ScriptObjects.ItemSO;
using UnityEngine;

namespace _Project.ScriptableObjects.ScriptObjects.ItemSO.TranqGunItem
{
    [CreateAssetMenu(fileName = "TranqGunItemSO", menuName = "Items/TranqGunItemSO")]
    public class TranqGunItemSO : BaseItemSO
    {
        [field: Header("TranqGun Data")]
        [field: SerializeField] public int AmmoAmount { get; private set; }
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField] public float DartSpeed { get; private set; }

    }

}
