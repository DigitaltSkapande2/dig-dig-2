using UnityEngine;

namespace DigDig2.CinemaCamera.CameraEffectors
{
	public class LockTargetInZoneEffector : TriggerZoneCameraEffectorBase
	{
		[SerializeField] private Transform targetTransform;

		protected override void OnZoneEnter( ) { }

		protected override void OnZoneExit( ) { }

		protected override void WhileActiveUpdate( )
		{
			if ( !targetTransform ) return;

			targetPosition = targetTransform.position;
			targetRotation = targetTransform.rotation;
		}
	}
}
