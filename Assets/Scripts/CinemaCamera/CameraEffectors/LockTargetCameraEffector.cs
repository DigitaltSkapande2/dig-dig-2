using System;
using UnityEngine;

namespace DigDig2.CinemaCamera.CameraEffectors
{
	public class LockTargetEffector : CameraEffector
	{
		[SerializeField] public Transform targetTransform;

        private void Awake()
        {
            Update();
        }

        private void Update( )
		{
			if ( !targetTransform ) return;

			targetPosition = targetTransform.position;
			targetRotation = targetTransform.rotation;
		}
	}
}
