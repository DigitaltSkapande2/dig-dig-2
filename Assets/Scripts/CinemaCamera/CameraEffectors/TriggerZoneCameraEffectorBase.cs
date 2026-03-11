using UnityEngine;

namespace DigDig2.CinemaCamera.CameraEffectors {
	public abstract class TriggerZoneCameraEffectorBase : CameraEffector {
		private new Collider collider;
		private Transform target;

		protected void Start( ) {
			Debug.Log( "ZINKKKKK" );

			collider = GetComponent<Collider>( );
			if ( collider == null ) Debug.LogError( "CinematicZoneEffector requires a Collider component." );

			target = GameCamera.Instance.GetComponent<Transform>( );
		}

		private void Update( ) {
			if ( !target || !collider ) return;

			bool shouldActive = collider.bounds.Contains( target.position );

			if ( shouldActive != IsActive ) {
				IsActive = shouldActive;
				if ( shouldActive )
					OnZoneEnter( );
				else
					OnZoneExit( );
			}

			if ( IsActive ) WhileActiveUpdate( );
		}

		protected abstract void OnZoneEnter( );
		protected abstract void OnZoneExit( );
		protected abstract void WhileActiveUpdate( );
	}
}
