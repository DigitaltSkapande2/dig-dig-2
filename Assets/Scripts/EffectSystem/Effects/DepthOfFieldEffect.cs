using DigDig2.CinemaCamera;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DigDig2.EffectSystem.Effects
{
	[RequireComponent( typeof( Volume ) )]
	public class DepthOfFieldEffect : MonoBehaviour
	{
		private DepthOfField depthOfField;

		private Camera mainCamera;
		private Volume volume;

		private void Start( )
		{
			volume = GetComponent<Volume>( );
			mainCamera = GameCamera.Instance.mainCamera;

			// Ensure the volume has a DepthOfField effect
			if ( volume.profile.TryGet( out depthOfField ) ) return;

			depthOfField = volume.profile.Add<DepthOfField>( );
			depthOfField.active = true;
		}

		private void Update( )
		{
			float cameraNearFarDistance = mainCamera.farClipPlane - mainCamera.nearClipPlane;
			float targetDepth = Mathf.Abs( mainCamera.transform.localPosition.z + mainCamera.nearClipPlane );
			depthOfField.focusDistance.value = targetDepth;
		}
	}
}
