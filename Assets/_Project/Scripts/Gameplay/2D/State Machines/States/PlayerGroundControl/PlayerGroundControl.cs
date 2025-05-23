using System.Collections;
using _Project.Scripts.Gameplay._2D.State_Machines.States.PlayerGroundControl.SubStates;
using _Project.Scripts.StateMachine;
using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.States.PlayerGroundControl
{
    public class PlayerGroundControl : State
    {
        [Header("State References")] public IdleState idleState;

        public RunState runState;

        [Header("Coyote Time Flags")] public bool coyoteTimeExpired;

        public bool isCoyoteTimerRunning;

        [Header("Core References")] private Player.Player PlayerCore => (Player.Player)Core;

        [Header("Properties")] private float CoyoteTimeDuration => PlayerCore.stats.coyoteTimeDurationSec;

        private float GroundGravity => PlayerCore.stats.groundGravity;

        public override void Enter()
        {
            ResetCoyoteTime();
            Body.gravityScale = GroundGravity;
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
            State nextState = PlayerCore.inputManager.IsMoving ? runState : idleState;
            SetChild(nextState, true);
        }

        private void HandleCoyoteTime()
        {
            if (PlayerCore.WantsToJump && PlayerCore.canJump) return;

            if (!Grounded && !isCoyoteTimerRunning) StartCoroutine(StartCoyoteTimeCountdown());

            if (coyoteTimeExpired) IsComplete = true;
            // Debug.Log("Coyote Time Expired");
        }

        private IEnumerator StartCoyoteTimeCountdown()
        {
            isCoyoteTimerRunning = true;
            yield return new WaitForSeconds(CoyoteTimeDuration);
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