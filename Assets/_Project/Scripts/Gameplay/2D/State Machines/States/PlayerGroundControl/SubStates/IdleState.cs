using _Project.Scripts.StateMachine;
using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.States.PlayerGroundControl.SubStates
{
    public class IdleState : State
    {
        public AnimationClip anim;
        public float animationSpeed = 1f;

        public override void Enter()
        {
            Animator.Play(anim.name);
            Animator.speed = animationSpeed;
        }

        public override void Do()
        {
            if (Grounded) IsComplete = true;
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