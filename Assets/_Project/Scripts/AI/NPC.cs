using UnityEngine;

namespace _Project.Scripts.AI
{
    public class NPC : Core.StateMachine.Core
    {
        public Patrol patrol;
        public Collect collect;


        void Start()
        {
            SetupInstances();
            SetState(patrol);
        }

        void Update()
        {
            if (state.isComplete)
            {
                if (state == collect)
                {
                    SetState(patrol);
                }
            }

            if (state == patrol)
            {
                collect.CheckForTarget();
                Debug.Log(collect.target);
                if (collect.target != null)
                {
                    SetState(collect);
                }
            }

            state.DoBranch();
        }

        void FixedUpdate()
        {
            state.FixedDoBranch();
        }
    }
}
