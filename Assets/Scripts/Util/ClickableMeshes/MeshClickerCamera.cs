using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DigDig2
{
    [RequireComponent(typeof(Camera))]
    public class MeshClickerCamera : MonoBehaviour, ProjectWideInputActions.IUIActions
    {
        private bool isPointerDown = false;

        private Camera camera;
        private Collider currentCollider;
        [SerializeField] private LayerMask clickableLayerMask;

        private void Start()
        {
            
            camera = GetComponent<Camera>();
        }

        void OnEnable()
        {
            InputManager.Instance.inputActions.UI.SetCallbacks(this);
        }

        void OnDisable()
        {
            InputManager.Instance.inputActions.UI.SetCallbacks(this);
        }

        private void FixedUpdate()
        {
            Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit raycastHit, 100f, clickableLayerMask))
            {
                if (raycastHit.collider != currentCollider)
                {
                    currentCollider?.GetComponent<ClickableMesh>()?.OnPointerClickRelease();
                    raycastHit.collider?.GetComponent<ClickableMesh>()?.OnPointerClickStart();
                }
            }
        }

        public void OnNavigate(InputAction.CallbackContext context)
        {

        }

        public void OnSubmit(InputAction.CallbackContext context)
        {

        }

        public void OnCancel(InputAction.CallbackContext context)
        {

        }

        public void OnPoint(InputAction.CallbackContext context)
        {

        }

        public void OnClick(InputAction.CallbackContext context)
        {
            Debug.Log("On Click " + context.ReadValueAsButton());
            if (isPointerDown != context.ReadValueAsButton())
            {
                isPointerDown = context.ReadValueAsButton();

                if (isPointerDown)
                {
                    currentCollider?.GetComponent<ClickableMesh>()?.OnPointerClickStart();
                }
                else
                {
                    currentCollider?.GetComponent<ClickableMesh>()?.OnPointerClickRelease();
                }
            }
        }

        public void OnRightClick(InputAction.CallbackContext context)
        {

        }

        public void OnMiddleClick(InputAction.CallbackContext context)
        {

        }

        public void OnScrollWheel(InputAction.CallbackContext context)
        {

        }

        public void OnTrackedDevicePosition(InputAction.CallbackContext context)
        {

        }

        public void OnTrackedDeviceOrientation(InputAction.CallbackContext context)
        {

        }
    }
}
