using _Project.Scripts.StateMachine;

namespace _Project.Scripts.Gameplay.AI
{
    public class Npc : StateMachineCore
    {
        public Patrol patrol;
        public Collect collect;


        private void Start()
        {
            InitializeStateMachine();
            SetState(patrol);
        }

        private void Update()
        {
            if (State.IsComplete)
                if (State == collect)
                    SetState(patrol);

            if (State == patrol)
            {
                collect.CheckForTarget();
                if (collect.target != null) SetState(collect);
            }

            State.DoBranch();
        }

        private void FixedUpdate()
        {
            State.FixedDoBranch();
        }
    }
}