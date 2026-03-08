
using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2
{
    [RequireComponent(typeof(Attacker))]
    public class PlayerAttackInput : MonoBehaviour, ProjectWideInputActions.IAttackActions
    {
        private ProjectWideInputActions.AttackActions attackActions;
        private bool hasStarted = false;

        private Attacker attacker;

        public bool InputEnabled => inputEnabled;
        private bool inputEnabled = false;



        private void Awake()
        {
            attacker = GetComponent<Attacker>();
        }
		private void Start()
        {
            if (!GameManager.Instance.Paused) EnableInput();
            GameManager.Instance.pauseStateChanged.AddListener((bool isPaused) =>
            {
                if (isPaused) DisableInput();
                else EnableInput();
            });
            
            hasStarted = true;
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
            attackActions = InputManager.Instance.inputActions.Attack;
            attackActions.SetCallbacks(this);
            inputEnabled = true;
        }

        private void DisableInput()
        {
            attackActions.RemoveCallbacks(this);
            inputEnabled = false;
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
