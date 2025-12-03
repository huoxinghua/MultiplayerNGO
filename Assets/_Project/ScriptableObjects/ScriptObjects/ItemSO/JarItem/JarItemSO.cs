using _Project.ScriptableObjects.ScriptObjects.ItemSO;
using UnityEngine;

namespace _Project.ScriptableObjects.ScriptObjects.ItemSO.JarItem
{
    [CreateAssetMenu(fileName = "JarItemSO", menuName = "Items/JarItemSO")]
    public class JarItemSO : BaseItemSO
    {
        [field: Header("Jar Data")]
        [field: SerializeField] public float CollectedAmount { get; private set; }
        [field: SerializeField] public bool HasCollected  { get; private set; }
    }

}