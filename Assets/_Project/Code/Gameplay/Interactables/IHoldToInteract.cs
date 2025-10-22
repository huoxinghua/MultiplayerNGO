using UnityEngine;

namespace _Project.Code.Gameplay.Interactables
{
    public interface IHoldToInteract
    {
        public void OnHold(GameObject player);
        public void OnRelease(GameObject player);
    }
}
