using System.Collections.Generic;

namespace _Project.Scripts.StateMachine
{
    public class StateMachine
    {
        public State State;

        public void Set(State newState, bool forceReset = false)
        {
            if (State == newState && !forceReset) return;
            State?.Exit();
            State = newState;
            State.Initialize(this);
            State.Enter();
        }

        public List<State> GetActiveStateBranch(List<State> list = null)
        {
            list ??= new List<State>();

            if (State == null) return list;

            list.Add(State);
            return State.Machine.GetActiveStateBranch(list);
        }
    }
}