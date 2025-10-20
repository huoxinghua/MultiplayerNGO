using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyPrefabsSO", menuName = "Enemy/EnemyPrefabsSO")]
public class EnemyPrefabsSO : ScriptableObject
{
    [field: SerializeField] public List<GameObject> TranquilPrefabs { get; private set; }
    [field: SerializeField] public List<GameObject> ViolentPrefabs { get; private set; }
    [field: SerializeField] public List<GameObject> HorrorPrefabs { get; private set; }
}
