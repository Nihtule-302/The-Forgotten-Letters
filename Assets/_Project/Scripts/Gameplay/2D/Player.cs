using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.StateMachine;
using _Project.Scripts.Gameplay._2D.State_Machines.States.Airborne;
using UnityEngine;

namespace _Project.Scripts.Gameplay._2D
{
    public class Player : StateMachineCore
    {
        // States
        public AirState airState;
        // public IdleState idleState;
        // public RunState runState;
        public GroundState groundState;

        // Serialized Fields
        public PlayerStats stats;
        [SerializeField] private InputManagerSO inputManager;

        // Input Variables
        public float xInput { get; private set; }
        public bool wantsToJump { get; private set; }

        // Unity Lifecycle Methods
        private void Start()
        {
            inputManager.Move += HandleXMovement;
            inputManager.Jump += HandleJumpInput;
            inputManager.EnablePlayerActions();

            SetupInstances();
            machine.Set(groundState);
        }

        private void Update()
        {
            SelectState();
            machine.state.DoBranch();
        }

        private void FixedUpdate()
        {
            ApplyGravity();
            ApplyFriction();
        }

        // State Management
        private void SelectState()
        {
            if (groundSensor.grounded)
            {
                // machine.Set(xInput == 0 ? idleState : runState);
                machine.Set(groundState);
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

        // Input Handling
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

        private void HandleJumpInput(bool jumpInput)
        {
            wantsToJump = jumpInput;

            if (wantsToJump && groundSensor.grounded)
            {
                body.linearVelocityY = stats.jumpSpeed;
            }
        }

        // Physics Methods
        private void ApplyGravity()
        {
            body.gravityScale = groundSensor.grounded ? 1 : stats.gravityForce;
        }

        private void ApplyFriction()
        {
            if (groundSensor.grounded && xInput == 0 && body.linearVelocityY <= 0.01f)
            {
                body.linearVelocityX *= stats.groundDecay;
            }
        }
    }
}
