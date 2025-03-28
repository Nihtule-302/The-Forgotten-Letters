using _Project.Scripts.Core.StateMachine;
using _Project.Scripts.Core.Utilities;
using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.States.Airborne
{
    public class AirState : State
    {
        public AnimationClip anim;
        public float jumpSpeed;

        public override void Enter()
        {
            animator.Play(anim.name);
        }
        public override void Do()
        {
            var time = Helpers.Map(body.linearVelocityY, jumpSpeed, -jumpSpeed, 0,1,true);
            animator.Play(anim.name,0,time);
            animator.speed = 0;
            if (grounded)
            {
                isComplete = true;
            }
        }
        public override void Exit()
        {

        }
    }
}
