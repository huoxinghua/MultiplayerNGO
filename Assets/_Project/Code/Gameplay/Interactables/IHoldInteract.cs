using UnityEngine;

namespace _Project.Code.Gameplay.Interactables
{
   public interface IHoldInteract
   {
      public void OnHold(GameObject interactingPlayer);
      public void OnRelease(GameObject interactingPlayer);
   }
}
