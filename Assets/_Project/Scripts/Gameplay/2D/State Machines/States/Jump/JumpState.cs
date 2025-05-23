using _Project.Scripts.Core.Utilities;
using _Project.Scripts.StateMachine;
using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.States.Jump
{
    public class JumpState : State
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
            var time = Helpers.Map(Body.linearVelocityY, jumpSpeed, 0, 0, 1, true);
            Animator.Play(anim.name, 0, time);
            Animator.speed = 0;
            if (Body.linearVelocityY < 0) IsComplete = true;
        }

        public override void Exit()
        {
        }
    }
}