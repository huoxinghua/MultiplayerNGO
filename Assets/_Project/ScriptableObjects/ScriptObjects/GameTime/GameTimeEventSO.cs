using _Project.Code.Gameplay;
using UnityEngine;

namespace _Project.ScriptableObjects.ScriptObjects.GameTime
{
    [CreateAssetMenu(fileName = "GameTimeEventSO", menuName = "GameTime/GameTimeEventSO")]
    public class GameTimeEventSO : ScriptableObject
    {
        [field: Header("GameTimeForEvent")]
        [field: SerializeField] public float TimeForEvent { get; private set; }

        [field: Header("GameTimeKey")]
        [field: SerializeField] public GameTimeEnum EventType { get; private set; }

    }
    
}