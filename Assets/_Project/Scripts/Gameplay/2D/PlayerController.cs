using System;
using TarodevController;
using Thirdparty.Tarodev_2D_Controller._Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Gameplay._2D
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [SerializeField] private ScriptableStats stats;
        public ScriptableStats Stats => stats;
        
        private Rigidbody2D _rb;
        public Rigidbody2D Rb => _rb;

        private CapsuleCollider2D _col;
        public FrameInput frameInput;
        public Vector2 frameVelocity;
        private bool _cachedQueryStartInColliders;

        // New Input System
        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _jumpAction;

        #region Interface

        public Vector2 FrameInput => frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;

        #endregion

        private float _time;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();

            // Initialize Input System
            _playerInput = GetComponent<PlayerInput>();
            _moveAction = _playerInput.actions["Move"];
            _jumpAction = _playerInput.actions["Jump"];

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            GatherInput();
        }

        private void GatherInput()
        {
            // Read input from the new Input System
            frameInput = new FrameInput
            {
                JumpDown = _jumpAction.WasPressedThisFrame(),
                JumpHeld = _jumpAction.IsPressed(),
                Move = _moveAction.ReadValue<Vector2>()
            };

            if (stats.SnapInput)
            {
                frameInput.Move.x = Mathf.Abs(frameInput.Move.x) < stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(frameInput.Move.x);
                frameInput.Move.y = Mathf.Abs(frameInput.Move.y) < stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(frameInput.Move.y);
            }

            if (frameInput.JumpDown)
            {
                jumpToConsume = true;
                timeJumpWasPressed = _time;
            }

            ApplyMovement();
        }

        private void FixedUpdate()
        {
            CheckCollisions();
            HandleDirection();
            HandleGravity();   
        }

        #region Collisions

        [SerializeField] private float frameLeftGrounded = float.MinValue;
        public bool grounded;
        

        private void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;

            // Ground and Ceiling
            bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, stats.GrounderDistance, ~stats.PlayerLayer);
            bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, stats.GrounderDistance, ~stats.PlayerLayer);

            // Hit a Ceiling
            if (ceilingHit) frameVelocity.y = Mathf.Min(0, frameVelocity.y);

            // Landed on the Ground
            if (!grounded && groundHit)
            {
                grounded = true;
                coyoteUsable = true;
                bufferedJumpUsable = true;
                endedJumpEarly = false;
                GroundedChanged?.Invoke(true, Mathf.Abs(frameVelocity.y));
            }
            // Left the Ground
            else if (grounded && !groundHit)
            {
                grounded = false;
                frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);
            }

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }

        #endregion

        #region Jumping

        public bool jumpToConsume;
        public bool bufferedJumpUsable;
        public bool endedJumpEarly;
        public bool coyoteUsable;
        public float timeJumpWasPressed;

        public bool HasBufferedJump => bufferedJumpUsable && _time < timeJumpWasPressed + stats.JumpBuffer;
        public bool CanUseCoyote => coyoteUsable && !grounded && _time < frameLeftGrounded + stats.CoyoteTime;


        public bool WantAndCanJump()
        {
            if (!endedJumpEarly && !grounded && !frameInput.JumpHeld && Rb.linearVelocity.y > 0) endedJumpEarly = true;

            if (!jumpToConsume && !HasBufferedJump) return false;

            return (grounded || CanUseCoyote);
        }

        public void OnJump()
        {
            Jumped?.Invoke();
        }

        #endregion

        #region moveHorizontal

        public bool WantToMove() => frameInput.Move.x != 0;
        public bool isStationary() => frameVelocity.x == 0; 

        private void HandleDirection()
        {
            if (frameInput.Move.x == 0)
            {
                var deceleration = grounded ? Stats.GroundDeceleration : Stats.AirDeceleration;
                frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);

            }
            else
            {
                frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, frameInput.Move.x * Stats.MaxSpeed, Stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Gravity

        private void HandleGravity()
        {
            if (grounded && frameVelocity.y <= 0f)
            {
                frameVelocity.y = stats.GroundingForce;
            }
            else
            {
                var inAirGravity = stats.FallAcceleration;
                if (endedJumpEarly && frameVelocity.y > 0) inAirGravity *= stats.JumpEndEarlyGravityModifier;
                frameVelocity.y = Mathf.MoveTowards(frameVelocity.y, -stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
            }
        }

        #endregion

        private void ApplyMovement() => _rb.linearVelocity = frameVelocity;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
        }
#endif
    }
    
}