using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2
{
    [RequireComponent(typeof(Attacker))]
    public class PlayerAttackInput : NetworkBehaviour, ProjectWideInputActions.IAttackActions
    {
        private Attacker attacker;

        private ProjectWideInputActions.AttackActions attackActions;

        private Vector2 mousePos;
        private Vector2 joystickVector;



        private void Awake()
        {
            attacker = GetComponent<Attacker>();
        }

		void Start()
        {
            if (isLocalPlayer) EnableInput();
        }

        #region Input Setup

        private void EnableInput()
        {
            Debug.Log($"Enabled Attack Input on {gameObject.name}");
            attackActions = InputManager.Instance.inputActions.Attack;

            attackActions.SetCallbacks(this);
        }

        private void DisableInput()
        {
            attackActions.RemoveCallbacks(this);
        }

        #endregion

        #region Inputs

        public void OnAttack1(InputAction.CallbackContext context)
        {
            if (context.performed) attacker.ClientRequestAttackStart(0);
            else attacker.ClientRequestAttackEnd();
        }
        public void OnAttack2(InputAction.CallbackContext context)
        {
            if (context.performed) attacker.ClientRequestAttackStart(1);
            else attacker.ClientRequestAttackEnd();
        }

        public void OnMouseAim(InputAction.CallbackContext context)
        {
            mousePos = context.ReadValue<Vector2>();
        }

        public void OnJoystickAim(InputAction.CallbackContext context)
        {
            joystickVector = context.ReadValue<Vector2>();
        }

        #endregion
    }
}
