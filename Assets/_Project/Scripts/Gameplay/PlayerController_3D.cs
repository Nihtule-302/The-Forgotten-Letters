using _Project.Scripts.Managers;
using UnityEngine;

namespace _Project.Scripts.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController3D : MonoBehaviour
    {
        
        [SerializeField] private float playerSpeed = 2.0f;
        [SerializeField] private float jumpHeight = 1.0f;
        [SerializeField] private float gravityValue = -9.81f;
        
        
        private CharacterController _controller;
        private Vector3 _playerVelocity;
        private bool _groundedPlayer;
        private InputManager _inputManager;
        private Transform camTransform;
        
        
        private void Start()
        {
            camTransform = Camera.main.transform; 
            _inputManager = InputManager.Instance;
            _controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            _groundedPlayer = _controller.isGrounded;
            if (_groundedPlayer && _playerVelocity.y < 0)
            {
                _playerVelocity.y = 0f;
            }

            Vector2 movement = _inputManager.GetMovement();
            Vector3 move = new Vector3(movement.x, 0, movement.y);
            move = camTransform.forward * move.z + camTransform.right * move.x;
            move.y = 0;
            _controller.Move(move * (Time.deltaTime * playerSpeed));

            // Makes the player jump
            if (_inputManager.PlayerJumpedThisFrame() && _groundedPlayer)
            {
                _playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
            }

            _playerVelocity.y += gravityValue * Time.deltaTime;
            _controller.Move(_playerVelocity * Time.deltaTime);
        }
    }
}
