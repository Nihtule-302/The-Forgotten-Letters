using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.Player_2D.States
{
    public class GroundedState : PlayerState
    {
        private float fallRecoverySec = 0f;
        public GroundedState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateFactory playerStateFactory) : base(stateMachine, player, playerStateFactory)
        {
            stateName = "Grounded";
        }

        public override void Enter()
        {
        }

        public override void Do()
        {
            if (time >= fallRecoverySec)
            {
                SwitchState(states.Idle());
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
            
        }
    }
}

