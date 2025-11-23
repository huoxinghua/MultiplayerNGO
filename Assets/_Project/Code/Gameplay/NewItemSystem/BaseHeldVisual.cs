using _Project.Code.Art.AnimationScripts.IK;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.NewItemSystem
{
   public class BaseHeldVisual : NetworkBehaviour
   {
      //Class to apply to all held visuals. Will neatly store the held visuals -> Renderer - IKInteractable - Other
      [field: SerializeField] public Renderer HeldRenderer { get; private set; }
      [field: SerializeField] public IKInteractable HeldIKInteractable { get; private set; }


      public void SetRendererActive(bool active)
      {
         HeldRenderer.enabled = active;
      }

      /// <summary>
      /// Handles setting the "pickup"/equip logic for IK with a single controller
      /// </summary>
      /// <param name="ikController">The IK controller (FPS or TPS) to set up</param>
      /// <param name="isFPS">True if this is the FPS controller, false for TPS</param>
      public void IKEquipped(PlayerIKController ikController, bool isFPS)
      {
        if (HeldIKInteractable == null || ikController == null)
        {
            Debug.LogError($"[{gameObject.name}] HeldIKInteractable or IKController is null!");
            return;
        }

        HeldIKInteractable.PickupAnimation(ikController, isFPS);
      }

      public void IKUnequipped()
      {
         if (HeldIKInteractable == null)
         {
             Debug.LogError($"[{gameObject.name}] HeldIKInteractable is not assigned! Please assign it in the Inspector on the held visual prefab.");
             return;
         }

         HeldIKInteractable.DropAnimation();
      }
   }
}
