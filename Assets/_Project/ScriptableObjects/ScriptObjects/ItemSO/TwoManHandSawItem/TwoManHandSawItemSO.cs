using _Project.ScriptableObjects.ScriptObjects.ItemSO;
using UnityEngine;

namespace _Project.ScriptableObjects.ScriptObjects.ItemSO.TwoManHandSawItem
{
    [CreateAssetMenu(fileName = "TwoManHandSawItemSO", menuName = "Items/TwoManHandSawItemSO")]
    public class TwoManHandSawItemSO : BaseItemSO
    {
        [field: Header("TwoManHandSaw Data")]
        [field: SerializeField] public float SawTimeAmount { get; private set; }
        [field: SerializeField] public bool PlayerCloseBy { get; private set; }
    }
}

