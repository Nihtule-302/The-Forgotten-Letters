using _Project.Scripts.Core.Utilities;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.StateMachine
{
    public abstract class StateMachineCore : MonoBehaviour
    {
        [Header("State Machine Components")] public Rigidbody2D body;

        public Animator animator;
        public GroundSensor groundSensor;
        public StateMachine Machine;

        public State State => Machine?.State;

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (Application.isPlaying && State != null)
            {
                var states = Machine?.GetActiveStateBranch();
                if (states != null && states.Count > 0)
                    Handles.Label(transform.position, "Active States: " + string.Join(" > ", states));
            }
#endif
        }

        protected void SetState(State newState, bool forceReset = false)
        {
            Machine.Set(newState, forceReset);
        }

        public void InitializeStateMachine()
        {
            Machine = new StateMachine();

            var allChildStates = GetComponentsInChildren<State>();
            foreach (var childState in allChildStates) childState.SetCore(this);
        }
    }
}