using _Project.Code.Core.Patterns;
using UnityEngine;

namespace _Project.Code.Utilities.Singletons
{
    public class LayerMasks : Singleton<LayerMasks>
    {
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private LayerMask _enemyMask;
        [SerializeField] private LayerMask _playerMask;
        [SerializeField] private LayerMask _interactableMask;

        [field: SerializeField] public LayerMask ObstructionMask { get; private set; }
        public LayerMask GroundMask => _groundMask;
        public LayerMask EnemyMask => _enemyMask;
        public LayerMask PlayerMask => _playerMask;
        public LayerMask InteractableMask => _interactableMask;
    }
}
