using UnityEngine;

namespace _Project.ScriptableObjects.ScriptObjects.StoreSO.VanSO
{
    [CreateAssetMenu(fileName = "VanSO", menuName = "StoreSO/VanSO")]
    public class VanSO : ScriptableObject
    {
        [field: SerializeField] public Vector3 VanSpawnPoint { get; private set; }
        [field: SerializeField] public Vector3 VanDestination { get; private set; }
        [field: SerializeField] public Vector3 VanPointToStartDropping { get; private set; }
        [field: SerializeField] public Vector3 VanPointToStopDropping { get; private set; }
        [field: SerializeField] public float VanSpeed { get; private set; }
        public float DistanceFromStartToDropItems => Vector3.Distance(VanSpawnPoint, VanPointToStartDropping);

        public float DistanceFromStartToStopDropping =>
            (DistanceFromStartToDropItems +
             Vector3.Distance(VanPointToStartDropping, VanPointToStopDropping));

        public float DistanceForDroppingItems => DistanceFromStartToStopDropping - DistanceFromStartToDropItems;
        public float DistanceToDestination => Vector3.Distance(VanDestination, VanSpawnPoint);
    }
}