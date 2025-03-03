using _Project.Scripts.Gameplay._2D;
using _Project.Scripts.Gameplay._2D.State_Machines.Player_2D;


public class AirborneState : PlayerState
{
    public AirborneState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateFactory playerStateFactory) : base(stateMachine, player, playerStateFactory)
    {
        stateName = "Airborne";
    }

    public override void Do()
    {
        
    }

    public override void Enter()
    {
        
    }

    public override void Exit()
    {
        
    }

    public override void FixedDo()
    {
        
    }

    public override void InitializeChildrenStates()
    {
        
    }
}
