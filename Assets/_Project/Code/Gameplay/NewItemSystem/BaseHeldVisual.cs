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
         if(IsOwner) HeldIKInteractable.PickupAnimation(ikData.FPSIKController);
        else HeldIKInteractable.PickupAnimation(ikData.TPSIKController);
      }
/// <summary>
/// Handles "drop"/unequip for IK
/// </summary>
      public void IKUnequipped()
      {
         HeldIKInteractable.DropAnimation();
      }
   }
}
