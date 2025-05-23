using _Project.Scripts.Core.Utilities;
using _Project.Scripts.StateMachine;
using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.States.Fall
{
    public class FallState : State
    {
        public AnimationClip anim;
        public float jumpSpeed;
        public float airGravity;

        public override void Enter()
        {
            Body.gravityScale = airGravity;
        }

        public override void Do()
        {
            var time = Helpers.Map(Body.linearVelocityY, 0, -jumpSpeed, 0, 1, true);
            Animator.Play(anim.name, 0, time);
            Animator.speed = 0;
            if (Grounded) IsComplete = true;
        }

        public override void Exit()
        {
        }
    }
}