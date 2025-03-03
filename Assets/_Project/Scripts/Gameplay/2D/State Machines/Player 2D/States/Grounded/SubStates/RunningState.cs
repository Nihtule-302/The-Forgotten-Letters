using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.Player_2D.States
{
    public class RunningState : PlayerState
    {
        public RunningState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateFactory playerStateFactory) : base(stateMachine, player, playerStateFactory)
        {
            stateName = "Running";
        }

        
        public override void Enter()
        {
        }

        public override void Do()
        {
            if (!player.WantToMove() && player.isStationary())
            {
                SwitchState(states.Idle());
            }

            if(player.WantAndCanJump())
            {
                SwitchState(states.Jumping());
            }
        }

        public override void FixedDo()
        {
        }

        public override void Exit()
        {
        }

        

        public override void InitializeChildrenStates()
        {
            throw new System.NotImplementedException();
        }
    }
}
