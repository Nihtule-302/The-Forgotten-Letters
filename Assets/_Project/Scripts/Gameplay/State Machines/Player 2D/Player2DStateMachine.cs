using _Project.Scripts.Gameplay.State_Machines.Player_2D.States;
using UnityEngine;

namespace _Project.Scripts.Gameplay.State_Machines.Player_2D
{
    public class Player2DStateMachine : MonoBehaviour
    {
        Player2DState _currentState;
        
        public readonly Player2DState GroundedState = new Player2DGroundedState();
        public readonly Player2DState AirborneState = new Player2DAirborneState();

        public readonly Player2DState IdleState = new Player2DIdleState();
        public readonly Player2DState MovingState = new Player2DMovingState();
        public readonly Player2DState JumpingState = new Player2DJumpingState();
        
        void Start()
        {
            _currentState = IdleState;
        }
        
        void Update()
        {
            _currentState.UpdateState(this);
        }

        public void SwitchState(Player2DState newState)
        {
            _currentState.ExitState(this);
            _currentState = newState;
            _currentState.EnterState(this);
        }
    }
}
