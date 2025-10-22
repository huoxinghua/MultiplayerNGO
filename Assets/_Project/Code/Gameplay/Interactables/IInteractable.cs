using UnityEngine;

namespace _Project.Code.Gameplay.Interactables
{
    public interface IInteractable
    {
        public void OnInteract(GameObject interactingPlayer)
        {

        }
        public void HandleHover(bool isHovering)
        {

        }
    }
}
