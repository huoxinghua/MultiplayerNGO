     namespace _Project.Code.Gameplay.NPC.Violent.Brute.RefactorBrute
    {
        public class BruteHeardPlayerState : BruteBaseState
        {
            public BruteHeardPlayerState(BruteStateMachine stateController) : base(stateController)
            {

            }
            public override void OnEnter()
            {
                StateController.TimesAlerted++; 
        
                // !!!! Need to replace two with a SO variable? yeah, that !!!!
                if(StateController.TimesAlerted >= BruteSO.TimesHeardBeforeAgro)
                {
                    StateController.TransitionTo(StateController.BruteChaseState);
                }
                else
                {
                    StateController.TransitionTo(StateController.BruteAlertState);
                }
            }
            public override void OnExit()
            {

            }

            public override void StateUpdate()
            {

            }
            public override void StateFixedUpdate()
            {

            }
            public override void OnHearPlayer()
            {

            }
        }
    }
