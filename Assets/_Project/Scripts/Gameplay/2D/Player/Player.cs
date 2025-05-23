using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Scriptable_Events.EventTypes.String;
using _Project.Scripts.Gameplay._2D.State_Machines.States.Attack;
using _Project.Scripts.Gameplay._2D.State_Machines.States.Fall;
using _Project.Scripts.Gameplay._2D.State_Machines.States.Jump;
using _Project.Scripts.Gameplay._2D.State_Machines.States.PlayerGroundControl;
using _Project.Scripts.Gameplay._2D.State_Machines.States.Skill.SubStates;
using _Project.Scripts.StateMachine;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay._2D.Player
{
    public class Player : StateMachineCore
    {
        [Header("States")] public PlayerGroundControl playerGroundControl;

        public AttackState attackState;
        public JumpState jumpState;
        public FallState fallState;
        public EnergyBlastState energyBlastState;

        [Header("Core Components")] public PlayerStats stats;

        public InputManagerSO inputManager;

        [FormerlySerializedAs("CurrentStateRef")] [Header("Events")] [SerializeField] private AssetReference currentStateRef;

        public bool canJump;

        [Header("Input Variables")] public float XInput { get; private set; }

        public bool WantsToJump { get; private set; }
        public bool WantsToAttack { get; private set; }
        public bool WantsToUseSkill { get; private set; }
        private StringEvent StringEvent => EventLoader.Instance.GetEvent<StringEvent>(currentStateRef);

        private void Start()
        {
            inputManager.Move += HandleXMovement;
            inputManager.Jump += HandleJumpInput;
            inputManager.Attack += HandleAttackInput;
            inputManager.Skill += useSkill => WantsToUseSkill = useSkill;
            inputManager.EnablePlayerActions();

            InitializeStateMachine();
            SetFallState();
        }

        private void Update()
        {
            SelectState();
            Machine.State.DoBranch();
            RaiseStateEvent();
        }

        private void FixedUpdate()
        {
            Machine.State.FixedDoBranch();
            ApplyFriction();
        }

        private void OnDestroy()
        {
            inputManager.Move -= HandleXMovement;
            inputManager.Jump -= HandleJumpInput;
            inputManager.Attack -= HandleAttackInput;
            inputManager.Skill -= useSkill => WantsToUseSkill = useSkill;
        }

        private void SelectState()
        {
            if (WantsToAttack)
            {
                SetState(attackState);
                WantsToAttack = false;
            }

            if (WantsToUseSkill)
            {
                SetState(energyBlastState);
                WantsToUseSkill = false;
            }

            if (Machine.State == playerGroundControl)
            {
                if (groundSensor.grounded && WantsToJump && canJump) Jump();

                if (!groundSensor.grounded && playerGroundControl.isCoyoteTimerRunning && canJump &&
                    WantsToJump) Jump();

                if (!groundSensor.grounded && playerGroundControl.coyoteTimeExpired) SetFallState();
            }

            if (Machine.State == fallState)
                if (groundSensor.grounded)
                {
                    SetState(playerGroundControl);
                    canJump = true;
                }

            if (Machine.State == jumpState)
                if (body.linearVelocityY < 0)
                    SetFallState();

            if (Machine.State == attackState)
            {
                if (Machine.State.IsComplete) SetFallState();
                if (groundSensor.grounded && WantsToJump && canJump) Jump(false);
            }

            if (Machine.State == energyBlastState)
            {
                if (Machine.State.IsComplete) SetFallState();
                if (groundSensor.grounded && WantsToJump && canJump) Jump(false);
            }
        }

        private void Jump(bool switchToJumpState = true)
        {
            PerformJump();
            if (switchToJumpState) SetJumpState();
            canJump = false;
        }

        // Helper methods to reduce redundancy
        private void SetJumpState()
        {
            jumpState.airGravity = stats.jumpGravity;
            jumpState.jumpSpeed = stats.jumpSpeed;
            Machine.Set(jumpState, true);
        }

        private void HandleJumpInput(bool jumpInput)
        {
            WantsToJump = jumpInput;
        }

        private void SetFallState()
        {
            fallState.airGravity = stats.fallGravity;
            fallState.jumpSpeed = stats.jumpSpeed;
            Machine.Set(fallState);
        }


        private void PerformJump()
        {
            body.linearVelocityY = stats.jumpSpeed;
        }

        private void RaiseStateEvent()
        {
            var states = Machine.GetActiveStateBranch();
            var statePath = string.Join(" > ", states);
            StringEvent.Raise(statePath);
        }

        private void HandleXMovement(Vector2 direction)
        {
            XInput = direction.x;
            if (Mathf.Abs(XInput) > 0)
            {
                body.linearVelocityX = XInput * stats.maxXSpeed;
                FaceInput();
            }
        }

        private void ApplyFriction()
        {
            if (groundSensor.grounded && XInput == 0 && body.linearVelocityY <= 0.01f)
                body.linearVelocityX *= stats.groundDecay;
        }

        private void FaceInput()
        {
            var currentYRotation = transform.eulerAngles.y;

            if (Mathf.Sign(XInput) == 1)
                currentYRotation = 0;
            else if (Mathf.Sign(XInput) == -1) currentYRotation = 180;

            // transform.localScale = new Vector3(Mathf.Sign(xInput), 1, 1);
            transform.eulerAngles = new Vector3(0, currentYRotation, 0);
        }

        private void HandleAttackInput(bool attackInput)
        {
            WantsToAttack = attackInput;
        }
    }
}