
using DigDig2.CinemaCamera;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2
{
    [RequireComponent(typeof(EntityCharacterController), typeof(SingleplayerCharacterSwitcher))]
    public class PlayerCharacterInputController : MonoBehaviour
    {
        // Input
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
        
        #region Input Action Callbacks

        public void OnMove(InputValue context)
        {
            inputMoveVector = context.Get<Vector2>();
        }

        public void OnInteract(InputValue context)
        {
            //if (interactor) interactor.SendInteraction(context.phase); // TODO: Fix interactions
        }

        public void OnSwitchCharacter(InputValue context)
        {
            if (context.isPressed && characterSwitching != null)
            {
                characterSwitching.SwitchCharacter();
            }
        }

        public void OnDash(InputValue context)
        {
            if (context.isPressed)
            {
                entityCharacterController.Dash();
            }
        }

        public void OnPause(InputValue context)
        {
            if (context.isPressed)
            {
                GameManager.Instance.Pause();
            }
        }

        #endregion
    }
}
