using _Project.Code.Gameplay.NewItemSystem;
using UnityEngine;

namespace _Project.Code.Gameplay.NPC.Violent.Brute
{
    public class BrutePiece : BaseInventoryItem
    {
        public void Awake()
        {
            _tranquilValue = Random.Range(0f, 1f);
            _violentValue = Random.Range(0f, 1f);
            _miscValue = Random.Range(0f, 1f);
        }
        private void Update()
        {
            if (_hasOwner)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }
        public override void UseItem()
        {

        }
        void OnEnable()
        {
            transform.parent = null;
        }
    }
}
