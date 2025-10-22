namespace _Project.Code.Utilities.StateMachine
{
    public abstract class BaseState
    {
        public abstract void OnEnter();

        public abstract void OnExit();

        public abstract void StateUpdate();

        public abstract void StateFixedUpdate();

    }
}
