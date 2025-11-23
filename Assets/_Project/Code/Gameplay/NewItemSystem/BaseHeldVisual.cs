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
        Debug.Log($"[{gameObject.name}] IKEquipped() called for {(isFPS ? "FPS" : "TPS")} controller");

        // Validate IKInteractable
        if (HeldIKInteractable == null)
        {
            Debug.LogError($"[{gameObject.name}] HeldIKInteractable is not assigned! Please assign it in the Inspector on the held visual prefab.");
            return;
        }

        // Validate IK controller
        if (ikController == null)
        {
            Debug.LogError($"[{gameObject.name}] IKController is null!");
            return;
        }

        // Setup IK with single controller
        HeldIKInteractable.PickupAnimation(ikController, isFPS);
        Debug.Log($"[{gameObject.name}] IKEquipped() complete for {(isFPS ? "FPS" : "TPS")} - Interactable: {ikController.Interactable != null}");
      }
      /// <summary>
      /// Handles "drop"/unequip for IK
      /// </summary>
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
