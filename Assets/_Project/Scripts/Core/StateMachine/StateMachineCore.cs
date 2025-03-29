using System.Collections.Generic;
using _Project.Scripts.Core.Utilities;
using UnityEngine;

namespace _Project.Scripts.Core.StateMachine
{
    public abstract class StateMachineCore : MonoBehaviour
    {
        public Rigidbody2D body;
        public Animator animator;
        public GroundSensor groundSensor;
        public StateMachine machine;

        public State state => machine.state;

        protected void SetState(State newState, bool forceReset = false)
        {
            machine.Set(newState, forceReset);
        }

        public void SetupInstances()
        {
            machine = new();

            State[] allChildStates = GetComponentsInChildren<State>();
            foreach (State state in allChildStates)
            {
                state.SetCore(this);
            }
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if(Application.isPlaying && state != null)
            {
                List<State> states = machine.GetActiveStateBranch();
                UnityEditor.Handles.Label(transform.position, "Active States: " + string.Join(" > ", states));
            }
#endif
        }

    }
}
