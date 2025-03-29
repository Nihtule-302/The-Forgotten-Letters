using _Project.Scripts.Core.StateMachine;
using UnityEngine;

namespace _Project.Scripts.Gameplay.AI
{
    public class NPC : StateMachineCore
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
