using System;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.StateMachine;
using _Project.Scripts.Core.Utilities;
using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.States.Grounded.SubStates
{
    public class RunState : State
    {
        public AnimationClip anim;
        public float maxXSpeed;

        [SerializeField] InputManagerSO inputManager;
        public float xInput{get; private set;}
        
        public override void Enter()
        {
            inputManager.Move += HandleXMovement;
            inputManager.EnablePlayerActions();
            animator.Play(anim.name);
        }
        public override void Do()
        {
            var velX = body.linearVelocityX;

            var time = Helpers.Map(MathF.Abs(velX), 0, maxXSpeed, 0,1.6f,true);
            animator.speed = time;

            if (grounded)
            {
                isComplete = true;
            }
        }


        private void HandleXMovement(Vector2 direction)
        {
            xInput = direction.x;
            var maxXSpeed = stats.maxXSpeed;
            if (Mathf.Abs(xInput) > 0)
            {
                body.linearVelocityX = xInput * maxXSpeed;
                FaceInput();
            }
        }
    }
}
