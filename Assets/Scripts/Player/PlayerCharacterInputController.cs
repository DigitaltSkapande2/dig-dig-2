using System;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2
{
    [RequireComponent(typeof(EntityCharacterController), typeof(SingleplayerCharacterSwitching))]
    public class PlayerCharacterInputController : NetworkBehaviour, ProjectWideInputActions.IPlayerActions
    {
        // Input
        private ProjectWideInputActions.PlayerActions playerActions;
        private bool hasStarted = false;

        // Character Controller
        private EntityCharacterController entityCharacterController;
        private Vector2 inputMoveVector = Vector2.zero;

        // Character Switching
        private SingleplayerCharacterSwitching characterSwitching;

        // Interactors
        private Interactor interactor;


        private void Awake()
        {
            entityCharacterController = GetComponent<EntityCharacterController>();
            characterSwitching = GetComponent<SingleplayerCharacterSwitching>();
            interactor = GetComponentInChildren<Interactor>();
        }

        private void Start()
        {
            if (!NetworkClient.active || isLocalPlayer)
            {
                EnableInput();
                hasStarted = true;

                DebugNotesManager.Instance.RegisterPlayerCharacterController(entityCharacterController);
            }
        }

        private void Update()
        {
            if (!NetworkClient.active || isLocalPlayer)
            {
                if (Camera.main)
                {
                    Vector3 rotatedInputMoveVector = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f) * new Vector3(inputMoveVector.x, 0f, inputMoveVector.y);
                    entityCharacterController.inputMoveVector = rotatedInputMoveVector;
                }
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
            if (!NetworkClient.active || isLocalPlayer)
            {
                playerActions = InputManager.Instance.inputActions.Player;
                playerActions.SetCallbacks(this);
            }
        }

        private void DisableInput()
        {
            playerActions.RemoveCallbacks(this);
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

        #endregion
    }
}
