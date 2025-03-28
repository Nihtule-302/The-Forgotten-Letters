using UnityEngine;

namespace _Project.Scripts.Core.StateMachine
{
    public abstract class State : MonoBehaviour
    {
        public bool isComplete {get; protected set;}

        protected float startTime;

        public float time => Time.time - startTime;

        protected Core core;

        protected Rigidbody2D body => core.body;
        protected Animator animator => core.animator;
        protected bool grounded => core.groundSensor.grounded;


        public StateMachine machine;
        protected StateMachine parent;
        public State childState => machine.state;

        protected void SetChild(State newState, bool forceReset = false)
        {
            machine.Set(newState, forceReset);
        }

        public void SetCore(Core _core)
        {
            machine = new StateMachine();
            core = _core;
        }


        public virtual void Enter(){}
        public virtual void Do(){}
        public virtual void FixedDo(){}
        public virtual void Exit(){}

        public void DoBranch()
        {
            Do();
            childState?.DoBranch();
        }

        public void FixedDoBranch()
        {
            FixedDo();
            childState?.FixedDoBranch();
        }

        public void Initialise(StateMachine _parent)
        {
            parent = _parent;
            isComplete = false;
            startTime = Time.time;
        }
    }
}
    