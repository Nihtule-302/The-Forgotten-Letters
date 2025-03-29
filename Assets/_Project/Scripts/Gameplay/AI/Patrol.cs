using _Project.Scripts.Core.StateMachine;
using _Project.Scripts.Gameplay._2D.State_Machines.States.Grounded.SubStates;
using UnityEngine;

namespace _Project.Scripts.Gameplay.AI
{
    public class Patrol : State
    {
        public Navigate navigate;
        public IdleState idle;
        public Transform anchor1;
        public Transform anchor2;

        void GoToNextDestination()
        {
            // Toggle between anchor1 and anchor2 as the next patrol destination
            if (navigate.destination == (Vector2)anchor1.position)
                navigate.destination = anchor2.position;
            else
                navigate.destination = anchor1.position;

            SetChild(navigate, true); // Set Navigate as the current state
        }

        public override void Enter()
        {
            // Set initial destination if needed and start the patrol
            if (navigate.destination == Vector2.zero)
                navigate.destination = anchor1.position;

            GoToNextDestination();
        }

        public override void Do()
        {
            if (machine.state == navigate)
            {
                if (navigate.isComplete)
                {
                    SetChild(idle, true); // Switch to idle after reaching destination
                    body.linearVelocity = new Vector2(0, body.linearVelocityY); // Stop horizontal motion
                }
            }
            else
            {
                // Re-check destination if idle for more than 1 second
                if (machine.state.time > 1)
                {
                    GoToNextDestination();
                }
            }
        }
    }
}
