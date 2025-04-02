using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Scriptable_Events.EventTypes.String;
using _Project.Scripts.Core.StateMachine;
using _Project.Scripts.Gameplay._2D.State_Machines.States.Airborne;
using _Project.Scripts.Gameplay._2D.State_Machines.States.Grounded;
using System;

namespace _Project.Scripts.Gameplay._2D
{
    public class Player : StateMachineCore
    {
        [Header("States")]
        public PlayerGroundControl playerGroundControl;
        public AirState airState;
        public JumpState jumpState;
        public FallState fallState;

        [Header("Core Components")]
        public PlayerStats stats;
        public InputManagerSO inputManager;

        [Header("Input Variables")]
        public float xInput { get; private set; }
        public bool wantsToJump { get; private set; }

        [Header("Events")]
        [SerializeField] private StringEvent stringEvent;
        public bool canJump;

        private void Start()
        {
            inputManager.Move += HandleXMovement;
            inputManager.Jump += HandleJumpInput;
            inputManager.EnablePlayerActions();

            InitializeStateMachine();
            SetFallState();
        }

        private void Update()
        {
            SelectState();
            machine.state.DoBranch();
            RaiseStateEvent();
        }

        private void FixedUpdate()
        {
            machine.state.FixedDoBranch();
            ApplyFriction();
        }

        private void SelectState()
        {
            if (machine.state == playerGroundControl)
            {
                if (groundSensor.grounded && wantsToJump && canJump)
                {
                    Jump();
                }

                if (!groundSensor.grounded && playerGroundControl.isCoyoteTimerRunning && canJump && wantsToJump)
                {
                    Jump();
                }

                if (!groundSensor.grounded && playerGroundControl.coyoteTimeExpired)
                {
                    SetFallState();
                }
            }

            if (machine.state == fallState)
            {
                if (groundSensor.grounded)
                {
                    SetState(playerGroundControl);
                    canJump = true;
                }
            }

            if (machine.state == jumpState)
            {
                if (body.linearVelocityY < 0)
                {
                    SetFallState();
                }
            }
        }

        private void Jump()
        {
            PerformJump();
            SetJumpState();
            canJump = false;
        }

        // Helper methods to reduce redundancy
        private void SetJumpState()
        {
            jumpState.airGravity = stats.jumpGravity;
            jumpState.jumpSpeed = stats.jumpSpeed;
            machine.Set(jumpState, true);
        }

        private void HandleJumpInput(bool jumpInput)
        {
            wantsToJump = jumpInput;
        }

        private void SetFallState()
        {
            fallState.airGravity = stats.fallGravity;
            fallState.jumpSpeed = stats.jumpSpeed;
            machine.Set(fallState);
        }


        private void PerformJump()
        {
            body.linearVelocityY = stats.jumpSpeed;
        }

        private void RaiseStateEvent()
        {
            List<State> states = machine.GetActiveStateBranch();
            string statePath = string.Join(" > ", states);
            stringEvent.Raise(statePath);
        }

        private void HandleXMovement(Vector2 direction)
        {
            xInput = direction.x;
            if (Mathf.Abs(xInput) > 0)
            {
                body.linearVelocityX = xInput * stats.maxXSpeed;
                FaceInput();
            }
        }

        private void ApplyFriction()
        {
            if (groundSensor.grounded && xInput == 0 && body.linearVelocityY <= 0.01f)
            {
                body.linearVelocityX *= stats.groundDecay;
            }
        }

        private void FaceInput()
        {
            transform.localScale = new Vector3(Mathf.Sign(xInput), 1, 1);
        }
    }
}
