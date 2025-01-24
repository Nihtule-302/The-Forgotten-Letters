using System;
using Thirdparty.Tarodev_2D_Controller._Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Managers
{
    public class InputManager : MonoBehaviour
    {
        
        private static InputManager _instance;
        
        public static InputManager Instance
        {
            get
            {
                return _instance;
            }
        }


        private PlayerControls _playerControls;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            else
            {
                _instance = this;
            }

            _playerControls = new PlayerControls();
        }

        private void OnEnable()
        {
            _playerControls.Enable();
        }

        private void OnDisable()
        {
            _playerControls.Disable();
        }
        
        public Vector2 GetMovement()
        {
            return _playerControls.Player.Move.ReadValue<Vector2>();
        }
        
        public Vector2 GetMouseDelta()
        {
            return _playerControls.Player.Look.ReadValue<Vector2>();
        }
        
        public bool PlayerJumpedThisFrame()
        {
            return _playerControls.Player.Jump.triggered;
        }
        
        
    }
}