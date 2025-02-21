namespace _Project.Scripts.Gameplay._2D.State_Machines.Player_2D
{
    public abstract class PlayerState
    {
        public abstract void EnterState(PlayerStateMachine stateMachine);
        public abstract void UpdateState(PlayerStateMachine stateMachine);
        public abstract void ExitState(PlayerStateMachine stateMachine);
    }
}
