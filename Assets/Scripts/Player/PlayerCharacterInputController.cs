
using DigDig2.CinemaCamera;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2
{
    [RequireComponent(typeof(EntityCharacterController), typeof(SingleplayerCharacterSwitcher))]
    public class PlayerCharacterInputController : MonoBehaviour, ProjectWideInputActions.IPlayerActions
    {
        // Input
        private ProjectWideInputActions.PlayerActions playerActions;
        private bool hasStarted = false;

        // Character Controller
        private EntityCharacterController entityCharacterController;
        private Vector2 inputMoveVector = Vector2.zero;

        // Character Switching
        private SingleplayerCharacterSwitcher characterSwitching;

        // Interactors
        private Interactor interactor;

        public bool InputEnabled => inputEnabled;
        private bool inputEnabled = false;

        private Camera mainCamera;


        private void Awake()
        {
            entityCharacterController = GetComponent<EntityCharacterController>();
            characterSwitching = GetComponent<SingleplayerCharacterSwitcher>();
            interactor = GetComponentInChildren<Interactor>();
        }

        private void Start()
        {
            if (!GameManager.Instance.Paused) EnableInput();
            GameManager.Instance.pauseStateChanged.AddListener((bool isPaused) =>
            {
                if (isPaused) DisableInput();
                else EnableInput();
            });
            
            mainCamera = GameCamera.Instance.mainCamera;
            hasStarted = true;
        }

        private void Update()
        {

            if (!inputEnabled)
            {
                entityCharacterController.inputMoveVector = Vector3.zero;
            }
            
            if (mainCamera != null)
            {
                Vector3 rotatedInputMoveVector = Quaternion.Euler(0f, GameCamera.Instance.mainCamera.transform.rotation.eulerAngles.y, 0f) * new Vector3(inputMoveVector.x, 0f, inputMoveVector.y);
                entityCharacterController.inputMoveVector = rotatedInputMoveVector;
            }
            else
            {
                mainCamera = GameCamera.Instance.mainCamera;
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
            playerActions = InputManager.Instance.inputActions.Player;
            playerActions.SetCallbacks(this);
            inputEnabled = true;
        }

        private void DisableInput()
        {
            playerActions.RemoveCallbacks(this);
            inputEnabled = false;
        }

        #endregion

        #region Input Action Callbacks

        public void OnMove(InputAction.CallbackContext context)
        {
            inputMoveVector = context.ReadValue<Vector2>();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (interactor) interactor.SendInteraction(context.phase);
        }

        public void OnSwitchCharacter(InputAction.CallbackContext context)
        {
            if (context.performed && characterSwitching != null)
            {
                characterSwitching.SwitchCharacter();
            }
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                entityCharacterController.Dash();
            }
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                GameManager.Instance.Pause();
            }
        }

        #endregion
    }
}
