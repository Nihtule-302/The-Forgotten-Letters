using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.Player_2D.States
{
    public class FallingState : PlayerState
    {
        bool isTrulyGrounded;

        public FallingState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateFactory playerStateFactory) : base(stateMachine, player, playerStateFactory)
        {
            stateName = "Falling";
        }

        public override void Enter()
        {
            player.GroundedChanged += OnGroundedChanged;
            
        }

        
        public override void Do()
        {
        }
        public override void FixedDo()
        {
            if (isTrulyGrounded)
            {
                SwitchState(states.Grounded());
            }
            HandleGravity();
        }

        public override void Exit()
        {
            player.GroundedChanged -= OnGroundedChanged;
        }

        private void OnGroundedChanged(bool grounded, float impact)
        {
            if (grounded) 
            {
                isTrulyGrounded = true;
            }
        }

        private void HandleGravity()
        {
            // if (grounded && _frameVelocity.y <= 0f)
            // {
            //     _frameVelocity.y = stats.GroundingForce;

            //     isComplete = true;
            // }
            // else
            // {
            //     var inAirGravity = stats.FallAcceleration;
            //     if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= stats.JumpEndEarlyGravityModifier;
            //     _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
            // }
        }

        public override void InitializeChildrenStates()
        {
        }
    }
}
