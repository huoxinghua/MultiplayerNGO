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

      /// <summary>
      /// Sets the held visuals renderer to active or not
      /// </summary>
      /// <param name="active">True = Active - False = InActive</param>
      public void SetRendererActive(bool active)
      {
         HeldRenderer.enabled = active;
      }

      /// <summary>
      /// Handles setting the "pickup"/equip logic for IK
      /// </summary>
      /// <param name="ikData">Class containing the players IKControllers</param>
      public void IKEquipped(PlayerIKData ikData)
      {
        Debug.Log($"[{gameObject.name}] IKEquipped() called");

        // Validate all required references
        if (HeldIKInteractable == null)
        {
            Debug.LogError($"[{gameObject.name}] HeldIKInteractable is not assigned! Please assign it in the Inspector on the held visual prefab.");
            return;
        }

        if (ikData == null)
        {
            Debug.LogError($"[{gameObject.name}] PlayerIKData is null! ThisPlayerIKData not assigned on PlayerInventory.");
            return;
        }

        if (ikData.FPSIKController == null)
        {
            Debug.LogError($"[{gameObject.name}] FPSIKController is not assigned! Please assign it in the Inspector on PlayerIKData.");
            return;
        }

        if (ikData.TPSIKController == null)
        {
            Debug.LogError($"[{gameObject.name}] TPSIKController is not assigned! Please assign it in the Inspector on PlayerIKData.");
            return;
        }

        Debug.Log($"[{gameObject.name}] Calling PickupAnimation for FPS and TPS IK controllers");
        HeldIKInteractable.PickupAnimation(ikData.FPSIKController, true);
        HeldIKInteractable.PickupAnimation(ikData.TPSIKController, false);
        Debug.Log($"[{gameObject.name}] IKEquipped() complete - FPS Interactable: {ikData.FPSIKController.Interactable != null}, TPS Interactable: {ikData.TPSIKController.Interactable != null}");
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
