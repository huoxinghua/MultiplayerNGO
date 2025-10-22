using UnityEngine;

namespace _Project.Code.Gameplay.Interactables
{
    public interface IInOutDoor
    {
        public Transform UseDoor();
        public float GetTimeToOpen();
    }
}
