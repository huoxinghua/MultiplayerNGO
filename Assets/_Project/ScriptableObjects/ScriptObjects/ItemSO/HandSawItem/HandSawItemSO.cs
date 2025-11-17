using _Project.ScriptableObjects.ScriptObjects.ItemSO;
using UnityEngine;

namespace _Project.ScriptableObjects.ScriptObjects.ItemSO.HandSawItem
{
    [CreateAssetMenu(fileName = "HandSawItemSO", menuName = "Items/HandSawItemSO")]
    public class HandSawItemSO : BaseItemSO
    {
        [field: Header("HandSaw Data")]
        [field: SerializeField] public float SawTimeAmount { get; private set; }
        [field: SerializeField] public bool BeingUsed { get; private set; }
    }
}
