using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2
{
    [RequireComponent(typeof(EntityCharacterController))]
    public class PlayerCharacterInputController : MonoBehaviour, GameInputSystem.IPlayerActions
    {
        // Input
        private GameInputSystem.PlayerActions playerActions;
        private bool hasStarted = false;

        // Character Controller
        private EntityCharacterController entityCharacterController;
        private Vector2 inputMoveVector = Vector2.zero;

        // Interactors
        private Interactor interactor;



        private void Awake()
        {
            entityCharacterController = GetComponent<EntityCharacterController>();
            interactor = GetComponentInChildren<Interactor>();
        }

        private void Start()
        {
            EnableInput();
            hasStarted = true;
        }

        private void Update()
        {
            if (Camera.main)
            {
                Vector3 rotatedInputMoveVector = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f) * new Vector3(inputMoveVector.x, 0f, inputMoveVector.y);
                entityCharacterController.inputMoveVector = rotatedInputMoveVector;
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

        private void EnableInput()
        {
            playerActions = GameInputManager.Instance.gameInputSystem.Player;

            playerActions.SetCallbacks(this);
            playerActions.Enable();
        }

        private void DisableInput()
        {
            playerActions.Disable();
        }

        #endregion

        #region Input Action Callbacks

        public void OnMove(InputAction.CallbackContext context)
        {
            inputMoveVector = context.ReadValue<Vector2>();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            GetComponent<PlayerAttack>().OnAttack(context);
        }

        public void OnMouse(InputAction.CallbackContext context)
        {
            GetComponent<PlayerAttack>().OnMouse(context);
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (interactor) interactor.SendInteraction(context.phase);
        }

        public void OnSprint(InputAction.CallbackContext context)
        {

        }

        #endregion
    }
}
