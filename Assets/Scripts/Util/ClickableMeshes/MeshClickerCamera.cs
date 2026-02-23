using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DigDig2
{
    [RequireComponent(typeof(Camera))]
    public class MeshClickerCamera : MonoBehaviour
    {
        private bool isPointerDown = false;

        private Camera camera;
        private Collider currentCollider;

        private void Start()
        {
            camera = GetComponent<Camera>();
        }

        private void FixedUpdate()
        {
            Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit raycastHit))
            {
                if (raycastHit.collider != currentCollider)
                {
                    currentCollider?.GetComponent<ClickableMesh>()?.OnPointerClickRelease();
                    raycastHit.collider?.GetComponent<ClickableMesh>()?.OnPointerClickStart();
                }
            }
        }

        public void OnClick(InputValue inputValue)
        {
            if (isPointerDown != inputValue.isPressed)
            {
                isPointerDown = inputValue.isPressed;

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
    }
}
