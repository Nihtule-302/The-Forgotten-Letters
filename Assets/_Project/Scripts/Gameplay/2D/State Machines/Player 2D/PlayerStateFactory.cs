using _Project.Scripts.Gameplay._2D.State_Machines.Player_2D.States;
using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.Player_2D
{
    public class PlayerStateFactory 
    {
        PlayerStateMachine machine;
        PlayerController Player => machine.Player;

        public PlayerStateFactory(PlayerStateMachine currentMachine)
        {
            machine = currentMachine;
        }

        public PlayerState Grounded()
        {
            return new GroundedState(machine, Player, this);
        }
        public PlayerState Idle()
        {
            return new IdleState(machine, Player, this);
        }
        public PlayerState Running()
        {
            return new RunningState(machine, Player, this);
        }


        public PlayerState Airborne()
        {
            return new AirborneState(machine, Player, this);
        }
        public PlayerState Jumping()
        {
            return new JumpingState(machine, Player, this);
        }
        public PlayerState Falling()
        {
            return new FallingState(machine, Player, this);
        }
        


        public PlayerState Attacking()
        {
            return new AttackingState(machine, Player, this);
        }


        public PlayerState Skill()
        {
            return new SkillState(machine, Player, this);
        }
        public PlayerState DashStrike()
        {
            return new DashStrikeState(machine, Player, this);
        }
        public PlayerState EnergyBlast()
        {
            return new EnergyBlastState(machine, Player, this);
        }
        public PlayerState Shockwave()
        {
            return new ShockwaveState(machine, Player, this);
        }

    }
}

