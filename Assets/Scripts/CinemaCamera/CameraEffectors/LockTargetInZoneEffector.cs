using UnityEngine;

namespace DigDig2.CinemaCamera.CameraEffectors
{
	public class LockTargetInZoneEffector : TriggerZoneCameraEffectorBase
	{
		[SerializeField] private Transform targetTransform;
		[SerializeField] private float targetFrustumSize = 10f;

		protected override void OnZoneEnter( ) { }

		protected override void OnZoneExit( ) { }

		protected override void WhileActiveUpdate( )
		{
			if ( !targetTransform ) return;

			position = targetTransform.position;
			rotation = targetTransform.rotation;
			frustumSize = targetFrustumSize;
		}
	}
}
