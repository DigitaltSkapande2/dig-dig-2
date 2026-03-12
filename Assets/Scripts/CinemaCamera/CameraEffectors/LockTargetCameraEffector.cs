using UnityEngine;

namespace DigDig2.CinemaCamera.CameraEffectors
{
	public class LockTargetEffector : CameraEffector
	{
		[SerializeField] public Transform targetTransform;

		private void Update( )
		{
			if ( !targetTransform ) return;

			position = targetTransform.position;
			rotation = targetTransform.rotation;
		}
	}
}
