using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerControls;

namespace _Project.Scripts.Core.Managers
{
    public interface IInputReader
    {
        Vector2 Direction { get; }
        void EnablePlayerActions();
        void DisablePlayerActions();
    }

    [CreateAssetMenu(fileName = "InputManagerSO", menuName = "InputManagerSO", order = 0)]
    public class InputManagerSO : ScriptableObject, IPlayerActions, IInputReader, IDrawingActions
    {
        public PlayerControls inputActions;
        public bool IsMoving => Mathf.Abs(Direction.x) > 0.01f;

        public bool IsJumpKeyPressed => inputActions.Player.Jump.IsPressed();
        public bool IsAttackKeyPressed => inputActions.Player.Attack.IsPressed();
        public bool IsSkillKeyPressed => inputActions.Player.Skill.IsPressed();
        public bool IsInteractKeyPressed => inputActions.Player.Interact.IsPressed();
        public bool IsClicking => inputActions.Drawing.Click.phase == InputActionPhase.Performed;

        public Vector2 DrawPointerPosition => inputActions.Drawing.PointerPosition.ReadValue<Vector2>();

        public void OnPointerPosition(InputAction.CallbackContext context)
        {
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    Click.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Click.Invoke(false);
                    break;
            }
        }

        public Vector2 Direction => inputActions.Player.Move.ReadValue<Vector2>();


        public void EnablePlayerActions()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerControls();
                inputActions.Player.SetCallbacks(this);
                inputActions.Drawing.SetCallbacks(this);
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
            switch (context.phase)
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
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    Attack.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Attack.Invoke(false);
                    break;
            }
        }

        public void OnSkill(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    Skill.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Skill.Invoke(false);
                    break;
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    Interact.Invoke(true);
                    break;
                case InputActionPhase.Canceled:
                    Interact.Invoke(false);
                    break;
            }
        }

        public void OnLook(InputAction.CallbackContext context)
        {
        }

        public void OnNext(InputAction.CallbackContext context)
        {
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
        }

        public event UnityAction<Vector2> Move = delegate { };
        public event UnityAction<bool> Jump = delegate { };

        public event UnityAction<bool> Attack = delegate { };
        public event UnityAction<bool> Skill = delegate { };
        public event UnityAction<bool> Interact = delegate { };


        public void OnCrouch(InputAction.CallbackContext context)
        {
        }


        public event UnityAction<bool> Click = delegate { };
    }
}