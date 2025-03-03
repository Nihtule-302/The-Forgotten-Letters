using Thirdparty.Tarodev_2D_Controller._Scripts;
using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.Player_2D.States
{
    public class IdleState : PlayerState
    {
        public IdleState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateFactory playerStateFactory) : base(stateMachine, player, playerStateFactory)
        {
            stateName = "Idle";
        }

        public override void Enter()
        {         
            
        }

        public override void Do()
        {
            if (player.WantAndCanJump())
            {
                SwitchState(states.Jumping());
            }

            if (player.WantToMove())
            {
                SwitchState(states.Running());
            }
        }

        public override void FixedDo()
        {
            
        }

        public override void Exit()
        {
            isComplete = true;
        }

        public override void InitializeChildrenStates(){}
    }
}
