using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.Player_2D.States
{
    internal class SkillState : PlayerState
    {
        public SkillState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateFactory playerStateFactory) : base(stateMachine, player, playerStateFactory)
        {
            stateName = "UsingSkill";
        }

        
        public override void Enter()
        {
        }

        public override void Do()
        {
            
            SwitchState(states.Idle());
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

