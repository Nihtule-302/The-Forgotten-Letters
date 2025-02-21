using _Project.Scripts.Gameplay._2D.State_Machines.Player_2D.States;
using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.Player_2D
{
    public class PlayerStateMachine : MonoBehaviour
    {
        PlayerState _currentState;
        
        public readonly PlayerState GroundedState = new PlayerGroundedState();
        public readonly PlayerState AirborneState = new PlayerAirborneState();

        public readonly PlayerState IdleState = new PlayerIdleState();
        public readonly PlayerState MovingState = new PlayerMovingState();
        public readonly PlayerState JumpingState = new PlayerJumpingState();
        
        void Start()
        {
            _currentState = IdleState;
        }
        
        void Update()
        {
            _currentState.UpdateState(this);
        }

        public void SwitchState(PlayerState newState)
        {
            _currentState.ExitState(this);
            _currentState = newState;
            _currentState.EnterState(this);
        }
    }
}
