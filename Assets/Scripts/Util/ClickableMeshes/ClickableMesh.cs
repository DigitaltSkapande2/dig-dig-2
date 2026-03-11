using UnityEngine;
using UnityEngine.Events;

namespace DigDig2.Util.ClickableMeshes {
	public class ClickableMesh : MonoBehaviour, IClickableMesh {
		public UnityEvent hoverEnter = new( );
		public UnityEvent hoverExit = new( );
		public UnityEvent clickStart = new( );
		public UnityEvent clickEnded = new( );

		public void OnPointerHoverEnter( ) { hoverEnter.Invoke( ); }

		public void OnPointerHoverExit( ) { hoverExit.Invoke( ); }

		public void OnPointerClickStart( ) { clickStart.Invoke( ); }

		public void OnPointerClickRelease( ) { clickEnded.Invoke( ); }
	}
}
