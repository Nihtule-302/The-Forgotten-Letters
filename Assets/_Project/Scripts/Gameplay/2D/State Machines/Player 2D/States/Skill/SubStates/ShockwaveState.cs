using _Project.Scripts.Gameplay._2D;
using _Project.Scripts.Gameplay._2D.State_Machines.Player_2D;

public class ShockwaveState : PlayerState
{
    public ShockwaveState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateFactory playerStateFactory) : base(stateMachine, player, playerStateFactory)
    {
        stateName = "Shockwave";
    }

    public override void Do()
    {
        throw new System.NotImplementedException();
    }

    public override void Enter()
    {
        throw new System.NotImplementedException();
    }

    public override void Exit()
    {
        throw new System.NotImplementedException();
    }

    public override void FixedDo()
    {
        throw new System.NotImplementedException();
    }

    public override void InitializeChildrenStates()
    {
        throw new System.NotImplementedException();
    }
}
