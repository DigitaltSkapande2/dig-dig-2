using DigDig2.CinemaCamera;

using UnityEngine;

namespace DigDig2.EffectSystem.Effects
{
	public class CameraZoomEffect : CumulativeEffectBase<CumulativeEffectInstanceData>
	{
		private CameraEffector cameraZoomEffector;

		private void Awake( )
		{
			var cameraZoomEffectorObject = new GameObject( );
			cameraZoomEffectorObject.transform.SetParent( transform );
			cameraZoomEffector = cameraZoomEffectorObject.AddComponent<CameraEffector>( );
		}

		internal override void UpdateEffect( float curveValue ) { cameraZoomEffector.targetFrustumSize = curveValue; }

		internal override void OnEffectEnd( CumulativeEffectInstanceData effect ) { cameraZoomEffector.targetFrustumSize = 0; }
	}
}
