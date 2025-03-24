using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerControls;

namespace _Project.Scripts.Core.Managers
{

    public interface IInputReader
    {
        Vector2 Direction {get; }
        void EnablePlayerActions();
        void DisablePlayerActions();
    }

    [CreateAssetMenu(fileName = "InputManagerSO", menuName = "InputManagerSO", order = 0)]
    public class InputManagerSO : ScriptableObject, IPlayerActions, IInputReader
    {
        public event UnityAction<Vector2> Move = delegate{};
        public event UnityAction<bool> Jump = delegate{};

        public event UnityAction<bool> Attack = delegate{};
        public event UnityAction<bool> Interact = delegate{};


        public PlayerControls inputActions;

        public Vector2 Direction => inputActions.Player.Move.ReadValue<Vector2>();
        public bool IsJumpKeyPressed => inputActions.Player.Jump.IsPressed();

        public bool IsAttackKeyPressed => inputActions.Player.Attack.IsPressed();
        public bool IsInteractKeyPressed => inputActions.Player.Interact.IsPressed();


        public void EnablePlayerActions()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerControls();
                inputActions.Player.SetCallbacks(this);
            }
            inputActions.Enable();
        }

        public void DisablePlayerActions()
        {
            if (inputActions == null) return;
            inputActions.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Move.Invoke(context.ReadValue<Vector2>());
        }
        public void OnJump(InputAction.CallbackContext context)
        {
            switch(context.phase)
            {
                case InputActionPhase.Started:
                    Jump.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Jump.Invoke(false);
                    break;
            }
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            switch(context.phase)
            {
                case InputActionPhase.Started:
                    Attack.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Attack.Invoke(false);
                    break;
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            switch(context.phase)
            {
                case InputActionPhase.Started:
                    Interact.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Interact.Invoke(false);
                    break;
            }
        }




        public void OnCrouch(InputAction.CallbackContext context){}  
        public void OnLook(InputAction.CallbackContext context){}
        public void OnNext(InputAction.CallbackContext context){}
        public void OnPrevious(InputAction.CallbackContext context){}
        public void OnSprint(InputAction.CallbackContext context){}
    }
}