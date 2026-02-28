using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2
{
    [RequireComponent(typeof(Attacker))]
    public class PlayerAttackInput : NetworkBehaviour, ProjectWideInputActions.IAttackActions
    {
        private ProjectWideInputActions.AttackActions attackActions;
        private bool hasStarted = false;

        private Attacker attacker;



        private void Awake()
        {
            attacker = GetComponent<Attacker>();
        }
		private void Start()
        {
            if (!NetworkClient.active || isLocalPlayer)
            {
                EnableInput();
                hasStarted = true;
            }
        }

        private void OnEnable()
        {
            if (hasStarted) EnableInput();
        }
        private void OnDisable()
        {
            DisableInput();
        }

        #region Input Setup

        public void EnableInput()
        {
            if (!NetworkClient.active || isLocalPlayer)
            {
                attackActions = InputManager.Instance.inputActions.Attack;
                attackActions.SetCallbacks(this);
                hasStarted = true;
            }
        }

        private void DisableInput()
        {
            attackActions.RemoveCallbacks(this);
        }

        #endregion

        #region Input Action Callbacks

        public void OnAttack1(InputAction.CallbackContext context)
        {
            if (context.performed) attacker.RequestAttackStart(0);
            else attacker.RequestAttackEnd();
        }
        public void OnAttack2(InputAction.CallbackContext context)
        {
            if (context.performed) attacker.RequestAttackStart(1);
            else attacker.RequestAttackEnd();
        }

        public void OnFocus(InputAction.CallbackContext context)
        {
            if (context.performed) attacker.StartFocus();
            else attacker.EndFocus();
        }

        public void OnFocusTarget(InputAction.CallbackContext context)
        {
            
        }

        #endregion
    }
}
