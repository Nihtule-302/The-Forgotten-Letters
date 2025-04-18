using _Project.Scripts.Core.StateMachine;
using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.States.Grounded.SubStates
{
    public class IdleState : State
    {
        public AnimationClip anim;
        public float animationSpeed = 1f;
        public override void Enter()
        {
            animator.Play(anim.name);
            animator.speed = animationSpeed;
        }
        public override void Do()
        {
            if (grounded)
            {
                isComplete = true;
            }
        }
        public override void FixedDo()
        {
            base.FixedDo();
        }
        public override void Exit()
        {
            base.Exit();
        }
    }
}
