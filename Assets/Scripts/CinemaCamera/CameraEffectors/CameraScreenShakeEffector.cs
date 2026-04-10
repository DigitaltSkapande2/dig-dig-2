using UnityEngine;
using UnityEngine.Serialization;

namespace DigDig2.CinemaCamera.CameraEffectors
{
	public class ScreenShakeEffector : PartialCameraEffector
	{
		[FormerlySerializedAs( "ShakeIntensity" )]
		[SerializeField] private float shakeIntensity;

		[SerializeField] private float shakeFrequency = 5;
		[SerializeField] private float shakeAmplitude = 5;

        private void Update( )
		{
			if ( shakeIntensity == 0f ) return;

			Vector2 shakeOffset = Vector2.zero;

			// -0.5 to make it center around 0, then *2 to make it -1 to 1
			shakeOffset.x = ( Mathf.PerlinNoise( Time.time * shakeFrequency, 0f ) - 0.5f ) * 2f * shakeAmplitude;
			shakeOffset.y = ( Mathf.PerlinNoise( 0f, Time.time * shakeFrequency ) - 0.5f ) * 2f * shakeAmplitude;

			shakeOffset *= shakeIntensity;

			lerpedRotation = Quaternion.Euler( shakeOffset.y, shakeOffset.x, 0f );

			// position 
			Vector2 offset = Random.insideUnitCircle * shakeIntensity;
			lerpedPosition = GameCamera.Instance.mainCamera.transform.up * offset.y + GameCamera.Instance.mainCamera.transform.right * offset.x;

			shakeIntensity = shakeIntensity < 0.001f ? 0f : Mathf.Lerp( shakeIntensity, 0f, Time.deltaTime * 2f );
		}

		public void Shake( float intensity ) { shakeIntensity += intensity; }
	}
}
