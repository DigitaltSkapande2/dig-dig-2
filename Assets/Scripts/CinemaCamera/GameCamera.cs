using DigDig2.SaveSystem;
using DigDig2.Util;

using Unity.Mathematics;

using UnityEngine;

namespace DigDig2.CinemaCamera {
	public class GameCamera : Singleton<GameCamera>, ISaveable {
		[Tooltip( "the time in seconds it will take for the camera to get to the target position" )]
		[SerializeField] public float followSpeed = 5f;

		[SerializeField] public float rotationSpeed = 1;

		public Camera mainCamera;
		private Quaternion baseTargetRotation;
		private float defaultFrustumHeight;
		private float targetFrustumSize;

		private Vector3 targetPos = Vector3.zero;
		private Quaternion targetRotation = Quaternion.identity;

		private void Start( ) {
			mainCamera = GetComponentInChildren<Camera>( );
			defaultFrustumHeight = mainCamera.orthographicSize;
		}

		private void Update( ) {
			float frustumSize = defaultFrustumHeight;

			foreach ( CameraEffector effector in CameraEffector.GetEffectivePivotCameraEffectors( ) ) {
				targetPos += effector.position;
				if ( effector.rotation.eulerAngles.magnitude > float.Epsilon ) targetRotation.eulerAngles += effector.rotation.eulerAngles;

				frustumSize += effector.frustumSize;
			}

			transform.position = Vector3.Lerp( transform.position, targetPos, followSpeed * Time.deltaTime );
			targetRotation.eulerAngles += targetRotation.eulerAngles;

			targetRotation = Quaternion.Slerp( targetRotation, baseTargetRotation, Time.deltaTime * rotationSpeed );

			transform.rotation = targetRotation;

			mainCamera.orthographicSize = frustumSize;
		}

		public void SetRotationSpeed( float speed ) { rotationSpeed = speed; }

		public void SetTargetRotation( float angle ) { SetTargetRotation( angle, false ); }

		public void SetTargetRotation( float angle, bool setInstant ) {
			baseTargetRotation = quaternion.Euler( 0, Mathf.Deg2Rad * angle, 0 );
			Debug.Log( $"Set Camera Rotation to {angle} {setInstant}" );
			if ( setInstant ) transform.rotation = baseTargetRotation;
		}

		#region Saving

		public object CollectData( ) => baseTargetRotation;

		public void RestoreState( object dataObject ) {
			if ( dataObject == null ) return;

			baseTargetRotation = (Quaternion)dataObject;
			transform.rotation = baseTargetRotation;
		}

		#endregion
	}
}
