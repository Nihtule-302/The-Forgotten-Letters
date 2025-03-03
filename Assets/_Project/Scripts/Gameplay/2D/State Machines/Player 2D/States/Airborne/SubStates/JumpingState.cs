using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.Player_2D.States
{
    public class JumpingState : PlayerState
    {
        public JumpingState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateFactory playerStateFactory) : base(stateMachine, player, playerStateFactory) 
        {
            stateName = "Jumping";
        }

        public override void Enter()
        {
            
        }

        public override void Do()
        {
        }
            

        public override void FixedDo()
        {
            if (player.WantAndCanJump())
            {
                HandleJump();
            }

            if(player.frameVelocity.y <= 0)
            {
                SwitchState(states.Falling());
            }
        }

        public override void Exit()
        {
            
        }


        private void HandleJump()
        {
            ExecuteJump();

            player.jumpToConsume = false;

            
        }

        private void ExecuteJump()
        {
            player.endedJumpEarly = false;
            player.timeJumpWasPressed = 0;
            player.bufferedJumpUsable = false;
            player.coyoteUsable = false;
            player.frameVelocity.y = player.Stats.JumpPower;
            player.OnJump();
        }

        public override void InitializeChildrenStates()
        {
        }
    }
}
