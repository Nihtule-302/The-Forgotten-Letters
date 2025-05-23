using System;
using _Project.Scripts.Core.Utilities;
using _Project.Scripts.StateMachine;
using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.States.PlayerGroundControl.SubStates
{
    public class RunState : State
    {
        public AnimationClip anim;
        public float maxXSpeed;

        public override void Enter()
        {
            Animator.Play(anim.name);
        }

        public override void Do()
        {
            var velX = Body.linearVelocityX;

            var time = Helpers.Map(MathF.Abs(velX), 0, maxXSpeed, 0, 1, true);
            Animator.speed = time;

            if (Grounded) IsComplete = true;
        }
    }
}