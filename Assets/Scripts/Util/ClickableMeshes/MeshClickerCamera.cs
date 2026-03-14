using DigDig2.Input;

using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2.Util.ClickableMeshes
{
	[RequireComponent( typeof( Camera ) )]
	public class MeshClickerCamera : MonoBehaviour
	{
		[SerializeField] private LayerMask clickableLayerMask;

		private new Camera camera;
		private Collider currentCollider;
		private bool isPointerDown;

		private void Start( ) { camera = GetComponent<Camera>( ); }

		private void FixedUpdate( )
		{
			Ray ray = camera.ScreenPointToRay( Mouse.current.position.ReadValue( ) );

			if ( !Physics.Raycast( ray, out RaycastHit raycastHit, 100f, clickableLayerMask ) ) return;

			if ( raycastHit.collider == currentCollider ) return;

			currentCollider?.GetComponent<ClickableMesh>( )?.OnPointerClickRelease( );
			raycastHit.collider?.GetComponent<ClickableMesh>( )?.OnPointerClickStart( );
		}

		public void OnInputUINavigate( InputInfo inputInfo ) { }

		public void OnInputUISubmit( InputInfo inputInfo ) { }

		public void OnInputUICancel( InputInfo inputInfo ) { }

		public void OnInputUIPoint( InputInfo inputInfo ) { }

		public void OnInputUIClick( InputInfo inputInfo )
		{
			// Debug.Log("On Click " + context.ReadValueAsButton());
			if ( isPointerDown == inputInfo.context.ReadValueAsButton( ) ) return;

			isPointerDown = inputInfo.context.ReadValueAsButton( );

			if ( isPointerDown )
				currentCollider?.GetComponent<ClickableMesh>( )?.OnPointerClickStart( );
			else
				currentCollider?.GetComponent<ClickableMesh>( )?.OnPointerClickRelease( );
		}

		public void OnInputUIRightClick( InputInfo inputInfo ) { }

		public void OnInputUIMiddleClick( InputInfo inputInfo ) { }

		public void OnInputUIScrollWheel( InputInfo inputInfo ) { }

		public void OnInputUITrackedDevicePosition( InputInfo inputInfo ) { }

		public void OnInputUITrackedDeviceOrientation( InputInfo inputInfo ) { }

		public void OnInputUIReset( InputInfo inputInfo ) { }
	}
}
