using _Project.Code.Art.AnimationScripts.Animations;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Utilities.StateMachine;
using UnityEngine;

namespace _Project.Code.Gameplay.Player.PlayerStateMachine
{
    public class PlayerBaseState : BaseState
    {
        protected PlayerStateMachine stateController;
        protected PlayerSO playerSO;
        protected CharacterController characterController;
        protected bool _jumpRequested = false;
        protected PlayerAnimation Animator;
        public PlayerBaseState(PlayerStateMachine stateController)
        {
            this.stateController = stateController;
            playerSO = stateController.PlayerSO;
            Animator = stateController.Animator;
            characterController = stateController.CharacterController;
        }
        public override void OnEnter()
        {
            //subscribe to input events
        }
        public override void OnExit()
        {
            //unsubscribe
        }

        public override void StateFixedUpdate()
        {

        }

        public override void StateUpdate()
        {
            HandleGravity();
            //HandleJump();
        }
        public virtual void OnCrouchInput()
        {

        }
        public virtual void OnSprintInput(bool isPerformed)
        {

        }
        public virtual void OnMoveInput(Vector2 movementDirection)
        {

        }

        public virtual void OnJumpInput(bool isPerformed)
        {
            stateController.JumpRequested = isPerformed;
            HandleJump();
        
        }

        #region Item Inputs (Default Implementation: Pass to Inventory Service)
        
        public virtual void OnUseInput()
        {
            if (stateController.Inventory != null)
                stateController.Inventory.UseItemInHand();
        }

        public virtual void OnSecondaryUseInput(bool isPressed)
        {
            if (stateController.Inventory != null)
                stateController.Inventory.SecondaryUseItemInHand(isPressed);
        }

        public virtual void OnDropItemInput()
        {
                stateController.Inventory.DropItem();
        }

        public virtual void OnInteractInput()
        {
            // TODO: Interaction logic will be handled here (e.g. calling an InteractionManager).
            // For now, this is empty as PlayerInventory does not have a public InteractWithEnvironment method.
        }

        public virtual void OnNumPressedInput(int slot)
        {
            if (stateController.Inventory != null)
                stateController.Inventory.EquipSlot(slot - 1); // Input is 1-based, Inventory expects 0-4
        }

        public virtual void OnChangeWeaponInput()
        {
            // TODO: Implement weapon swap logic (e.g., stateController.Inventory.SwapBigItemForWeapon())
            // once PlayerInventory has the public API for it.
        }

        #endregion

        public virtual void HandleJump()
        {

            if (stateController.CanJump && stateController.JumpRequested)
            {
                if (stateController.VerticalVelocity.y < 0f)
                    stateController.VerticalVelocity.y = -2f;
                stateController.VerticalVelocity.y = Mathf.Sqrt(playerSO.JumpStrength * -2f * playerSO.PlayerGravity);
                Animator.PlayJump();
            }


        }
        public virtual void HandleGravity()
        {
            stateController.VerticalVelocity.y += playerSO.PlayerGravity * Time.deltaTime;
            characterController.Move(stateController.VerticalVelocity * Time.deltaTime);

        }
    }
}