using _Project.Scripts.Core.Managers;
using _Project.Scripts.Gameplay._2D.State_Machines.States.Airborne;
using _Project.Scripts.Gameplay._2D.State_Machines.States.Grounded.SubStates;
using UnityEngine;

namespace _Project.Scripts.Gameplay._2D
{
    public class PlayerMovement : Core.StateMachine.Core
    {
        public AirState airState;
        public IdleState idleState;
        public RunState runState;

        // Serialized fields
        public PlayerStats stats;
    
        // Input variables
        [SerializeField] InputManagerSO inputManager;
        public float xInput{get; private set;}
        public bool wantsToJump{get; private set;}

        // Unity lifecycle methods
        void Start()
        {
            inputManager.Jump += HandleJumpInput;
            inputManager.EnablePlayerActions();

            SetupInstances();
            machine.Set(idleState);
        }

        void Update()
        {
            SelectState();   
            machine.state.DoBranch();
        }

        void FixedUpdate()
        {
            ApplyGravity();
            ApplyFriction();
        }

        // State management
        private void SelectState()
        {
            if (groundSensor.grounded)
            {
                if (xInput == 0)
                {
                    machine.Set(idleState);
                }
                else
                {
                    machine.Set(runState);
                }
            }
            else
            {
                machine.Set(airState);
            }
        }

        private void FaceInput()
        {
            var directionToFace = Mathf.Sign(xInput);
            transform.localScale = new Vector3(directionToFace, 1, 1);
        }

        private void HandleJumpInput(bool jumpInput)
        {
            wantsToJump = jumpInput;
            var jumpSpeed = stats.jumpSpeed;
            if (wantsToJump && groundSensor.grounded)
            {
                body.linearVelocityY = jumpSpeed;
            }
        }

        // Physics methods

        private void ApplyGravity()
        {
            var gravity = stats.gravityForce;

            if (groundSensor.grounded)
                body.gravityScale = 1;
            else
                body.gravityScale = gravity;
        }

        private void ApplyFriction()
        {
            var groundDecay = stats.groundDecay;

            if (groundSensor.grounded && xInput == 0 && body.linearVelocityY <= 0.01)
            {
                body.linearVelocityX *= groundDecay;
            }
        }
    }
}