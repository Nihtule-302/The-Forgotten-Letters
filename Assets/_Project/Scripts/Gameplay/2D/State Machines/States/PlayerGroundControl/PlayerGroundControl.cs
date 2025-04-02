using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using _Project.Scripts.Core.StateMachine;
using _Project.Scripts.Gameplay._2D.State_Machines.States.Grounded.SubStates;
using System.Collections;

namespace _Project.Scripts.Gameplay._2D.State_Machines.States.Grounded
{
    public class PlayerGroundControl : State
    {
        [Header("State References")]
        public IdleState idleState;
        public RunState runState;

        [Header("Core References")]
        private Player playerCore => (Player)core;

        [Header("Properties")]
        private float coyoteTimeDuration => playerCore.stats.coyoteTimeDurationSec;
        private float groundGravity => playerCore.stats.groundGravity;

        [Header("Coyote Time Flags")]
        public bool coyoteTimeExpired;
        public bool isCoyoteTimerRunning;

        public override void Enter()
        {
            ResetCoyoteTime();
            body.gravityScale = groundGravity;
        }

        public override void Do()
        {
            HandleMovement();
            HandleCoyoteTime();
        }

        public override void Exit()
        {
            ResetCoyoteTime();
        }

        private void HandleMovement()
        {
            State nextState = playerCore.inputManager.IsMoving ? runState : idleState;
            SetChild(nextState, true);
        }

        private void HandleCoyoteTime()
        {
            if (playerCore.wantsToJump && playerCore.canJump)
            {
                return;
            }

            if (!grounded && !isCoyoteTimerRunning)
            {
                StartCoroutine(StartCoyoteTimeCountdown());
            }

            if (coyoteTimeExpired)
            {
                isComplete = true;
                Debug.Log("Coyote Time Expired");
            }
        }

        private IEnumerator StartCoyoteTimeCountdown()
        {
            isCoyoteTimerRunning = true;
            yield return new WaitForSeconds(coyoteTimeDuration);
            coyoteTimeExpired = true;
            isCoyoteTimerRunning = false;
        }


        private void ResetCoyoteTime()
        {
            coyoteTimeExpired = false;
            isCoyoteTimerRunning = false;
        }
    }
}
