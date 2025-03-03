using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.Player_2D.States
{
    public class AttackingState : PlayerState
    {
        public AttackingState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateFactory playerStateFactory) : base(stateMachine, player, playerStateFactory)
        {
            stateName = "Attack";
        }

        public override void Do()
        {
            SwitchState(states.Idle());
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
}

