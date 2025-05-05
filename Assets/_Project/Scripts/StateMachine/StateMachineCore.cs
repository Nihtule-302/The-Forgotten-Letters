using System.Collections.Generic;
using _Project.Scripts.Core.Utilities;
using UnityEngine;

namespace _Project.Scripts.Core.StateMachine
{
    public abstract class StateMachineCore : MonoBehaviour
    {
        [Header("State Machine Components")]
        public Rigidbody2D body;
        public Animator animator;
        public GroundSensor groundSensor;
        public StateMachine machine;

        public State state => machine?.state;

        protected void SetState(State newState, bool forceReset = false)
        {
            machine.Set(newState, forceReset);
        }

        public void InitializeStateMachine()
        {
            machine = new StateMachine();

            State[] allChildStates = GetComponentsInChildren<State>();
            foreach (State childState in allChildStates)
            {
                childState.SetCore(this);
            }
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (Application.isPlaying && state != null)
            {
                List<State> states = machine?.GetActiveStateBranch();
                if (states != null && states.Count > 0)
                {
                    UnityEditor.Handles.Label(transform.position, "Active States: " + string.Join(" > ", states));
                }
            }
#endif
        }
    }
}
