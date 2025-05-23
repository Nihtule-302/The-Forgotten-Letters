using UnityEngine;

namespace _Project.Scripts.StateMachine
{
    public abstract class State : MonoBehaviour
    {
        protected StateMachineCore Core;
        protected StateMachine Parent;

        protected float StartTime;

        [Header("State Management")] public bool IsComplete { get; protected set; }

        public float Time => UnityEngine.Time.time - StartTime;
        public StateMachine Machine { get; private set; }

        protected Rigidbody2D Body => Core.body;
        protected Animator Animator => Core.animator;
        protected bool Grounded => Core.groundSensor.grounded;

        public State ChildState => Machine?.State;

        protected void SetChild(State newState, bool forceReset = false)
        {
            Machine?.Set(newState, forceReset);
        }

        public void SetCore(StateMachineCore core)
        {
            Machine = new StateMachine();
            Core = core;
        }

        public virtual void Enter()
        {
        }

        public virtual void Do()
        {
        }

        public virtual void FixedDo()
        {
        }

        public virtual void Exit()
        {
        }

        public void DoBranch()
        {
            Do();
            ChildState?.DoBranch();
        }

        public void FixedDoBranch()
        {
            FixedDo();
            ChildState?.FixedDoBranch();
        }

        public void Initialize(StateMachine parent)
        {
            Parent = parent;
            IsComplete = false;
            StartTime = UnityEngine.Time.time;
        }

        public override string ToString()
        {
            return GetType().Name;
            // Returns only the class name
        }
    }
}