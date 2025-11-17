using _Project.ScriptableObjects.ScriptObjects.ItemSO;
using UnityEngine;

namespace _Project.ScriptableObjects.ScriptObjects.ItemSO.TestTubeItem
{
    [CreateAssetMenu(fileName = "TestTubeItemSO", menuName = "Items/TestTubeItemSO")]
    public class TestTubeItemSO : BaseItemSO
    {
        [field: Header("TestTube Data")]
        [field: SerializeField] public bool HasCollected  { get; private set; }
    }

}
