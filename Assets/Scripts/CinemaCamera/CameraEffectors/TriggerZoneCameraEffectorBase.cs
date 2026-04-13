using DigDig2.Debugging;
using UnityEngine;

namespace DigDig2.CinemaCamera.CameraEffectors
{
	public abstract class TriggerZoneCameraEffectorBase : CameraEffector
	{
		private new Collider collider;
		private Transform target;

		protected override void Start( )
		{
			base.Start( );
			collider = GetComponent<Collider>( );
			if ( collider == null ) BetterDebug.Log( "CinematicZoneEffector requires a Collider component.", LogSeverity.Error);

			target = GameCamera.Instance.GetComponent<Transform>( );
		}

		private void Update( )
		{
			if ( !target || !collider ) return;

			bool shouldActive = collider.bounds.Contains( target.position );

			if ( shouldActive != IsActive )
			{
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
