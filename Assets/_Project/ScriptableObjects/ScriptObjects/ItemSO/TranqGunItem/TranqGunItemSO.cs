using _Project.ScriptableObjects.ScriptObjects.ItemSO;
using UnityEngine;

namespace _Project.ScriptableObjects.ScriptObjects.ItemSO.TranqGunItem
{
    [CreateAssetMenu(fileName = "TranqGunItemSO", menuName = "Items/TranqGunItemSO")]
    public class TranqGunItemSO : BaseItemSO
    {
        [field: Header("TranqGun Data")]
        [field: SerializeField] public float TimeBetweenShots { get; private set; }
        [field: SerializeField] public int AmmoAmount { get; private set; }
        [field: SerializeField] public float SpeedOfDart { get; private set; }
        [field: SerializeField] public bool HasBeenShot { get; private set; }
    }

}
