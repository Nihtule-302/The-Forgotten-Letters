using _Project.Scripts.Core.Managers;
using UnityEngine;

namespace _Project.Scripts.Gameplay._3D
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float playerSpeed = 2.0f;
        [SerializeField] private float jumpHeight = 1.0f;
        [SerializeField] private float gravityValue = -9.81f;
        [SerializeField] private InputManagerSO input;

        private CharacterController _controller;
        private Vector3 _playerVelocity;
        private bool _groundedPlayer;
        private Transform _camTransform;

        private void Start()
        {
            InitializeComponents();
            EnablePlayerInput();
        }

        private void InitializeComponents()
        {
            if (Camera.main != null) _camTransform = Camera.main.transform;
            _controller = GetComponent<CharacterController>();
        }

        private void EnablePlayerInput()
        {
            input?.EnablePlayerActions();
        }

        private void Update()
        {
            CheckGroundStatus();
            //HandleMovement();
            ApplyGravity();
        }


        // Check if the player is grounded and reset vertical velocity if grounded
        private void CheckGroundStatus()
        {
            _groundedPlayer = _controller.isGrounded;
            if (_groundedPlayer && _playerVelocity.y < 0)
            {
                _playerVelocity.y = 0f;
            }
        }

        // Handle player movement based on input and camera orientation
        private void HandleMovement()
        {
            Vector2 movementInput = input.Direction;
            Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);

            // Adjust movement to align with the camera's forward and right directions
            move = _camTransform.forward * move.z + _camTransform.right * move.x;
            move.y = 0;  // Keep movement on the horizontal plane

            _controller.Move(move * (Time.deltaTime * playerSpeed));
        }

        // Apply gravity to the player
        private void ApplyGravity()
        {
            _playerVelocity.y += gravityValue * Time.deltaTime;
            _controller.Move(_playerVelocity * Time.deltaTime);
        }

        // Uncomment to implement jump functionality
        // private void HandleJump()
        // {
        //     if (input.IsJumpKeyPressed && _groundedPlayer)
        //     {
        //         _playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        //     }
        // }
    }
}
