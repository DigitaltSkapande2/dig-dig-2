using DigDig2.Input;

using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2.Util.ClickableMeshes {
	[RequireComponent( typeof( Camera ) )]
	public class MeshClickerCamera : MonoBehaviour, ProjectWideInputActions.IUIActions {
		[SerializeField] private LayerMask clickableLayerMask;

		private new Camera camera;
		private Collider currentCollider;
		private bool isPointerDown;

		private void Start( ) { camera = GetComponent<Camera>( ); }

		private void FixedUpdate( ) {
			Ray ray = camera.ScreenPointToRay( Mouse.current.position.ReadValue( ) );

			if ( !Physics.Raycast( ray, out RaycastHit raycastHit, 100f, clickableLayerMask ) ) return;

			if ( raycastHit.collider == currentCollider ) return;

			currentCollider?.GetComponent<ClickableMesh>( )?.OnPointerClickRelease( );
			raycastHit.collider?.GetComponent<ClickableMesh>( )?.OnPointerClickStart( );
		}

		private void OnEnable( ) { InputManager.Instance.inputActions.UI.SetCallbacks( this ); }

		private void OnDisable( ) { InputManager.Instance.inputActions.UI.SetCallbacks( this ); }

		public void OnNavigate( InputAction.CallbackContext context ) { }

		public void OnSubmit( InputAction.CallbackContext context ) { }

		public void OnCancel( InputAction.CallbackContext context ) { }

		public void OnPoint( InputAction.CallbackContext context ) { }

		public void OnClick( InputAction.CallbackContext context ) {
			// Debug.Log("On Click " + context.ReadValueAsButton());
			if ( isPointerDown == context.ReadValueAsButton( ) ) return;

			isPointerDown = context.ReadValueAsButton( );

			if ( isPointerDown )
				currentCollider?.GetComponent<ClickableMesh>( )?.OnPointerClickStart( );
			else
				currentCollider?.GetComponent<ClickableMesh>( )?.OnPointerClickRelease( );
		}

		public void OnRightClick( InputAction.CallbackContext context ) { }

		public void OnMiddleClick( InputAction.CallbackContext context ) { }

		public void OnScrollWheel( InputAction.CallbackContext context ) { }

		public void OnTrackedDevicePosition( InputAction.CallbackContext context ) { }

		public void OnTrackedDeviceOrientation( InputAction.CallbackContext context ) { }

		public void OnReset( InputAction.CallbackContext context ) { }
	}
}
