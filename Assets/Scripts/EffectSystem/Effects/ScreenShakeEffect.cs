using System.Collections.Generic;
using System.Linq;
using DigDig2.CinemaCamera;
using DigDig2.Debugging;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DigDig2.EffectSystem.Effects
{
	public class ScreenShakeEffect : CumulativeEffectBase<CumulativeEffectInstanceData>
	{
		[SerializeField] private float shakeFrequency = 5;
		[SerializeField] private float shakeAmplitude = 5;
		private PartialCameraEffector screenShakeEffector;

        private List<float> pys = new();

		private void Awake( )
		{
			var screenShakeEffectorObject = new GameObject( "ScreenShakeEffector" );
			screenShakeEffectorObject.transform.SetParent( transform );
			screenShakeEffector = screenShakeEffectorObject.AddComponent<PartialCameraEffector>( );
		}

		internal override void UpdateEffect( float curveValue )
		{
			if ( curveValue <= 0f )
			{
				// reset when no shake
				screenShakeEffector.targetRotation = Quaternion.identity;
				screenShakeEffector.targetPosition = Vector3.zero;
				return;
			}

			// Use unscaled time so shake remains smooth when timeScale changes
			float t = Time.unscaledTime * shakeFrequency;
            
            // position: smooth perlin-based planar offset (in camera local plane)
            
            float px = (Mathf.PerlinNoise(t + 37.1f, 9.2f) - Mathf.PerlinNoise(t + 137.1f, 9.2f)) * shakeAmplitude * curveValue;
            float py = (Mathf.PerlinNoise(21.7f, t + 9.2f) - Mathf.PerlinNoise(221.7f, t + 9.2f)) * shakeAmplitude * curveValue;
            pys.Add(py);
            Vector3 planarOffset = (GameCamera.Instance.mainCamera.transform.up * py + GameCamera.Instance.mainCamera.transform.right * px);
            screenShakeEffector.targetPosition = planarOffset;

			// Smooth Perlin-based rotation and position for natural motion
			// rotation: use small pitch and roll (avoid large yaw changes that look jarring)
			float rotPitch = ( Mathf.PerlinNoise( t, 0f ) - 0.5f ) * 2f * ( shakeAmplitude * 0.5f ) * curveValue; // X-axis
			float rotRoll = ( Mathf.PerlinNoise( 0f, t ) - 0.5f ) * 2f * ( shakeAmplitude * 0.5f ) * curveValue; // Z-axis
            
            
			// Apply rotation as small offsets around local axes (pitch, roll)
			screenShakeEffector.targetRotation = Quaternion.Euler( rotPitch, 0f, rotRoll );
            
            //BetterDebug.Log(pys.Sum()/pys.Count);
		}

        internal override void OnEffectEnd(CumulativeEffectInstanceData effect)
        {
            if ( effectInstances.Count > 1 ) return;
            
            screenShakeEffector.targetRotation = Quaternion.identity;
            screenShakeEffector.targetPosition = Vector3.zero;
        }
    }

	#if UNITY_EDITOR
	[CustomEditor( typeof( ScreenShakeEffect ) )]
	public class ScreenShakeEffectEditor : Editor
	{
		public override void OnInspectorGUI( )
		{
			base.OnInspectorGUI( );
			var effect = (ScreenShakeEffect)target;
			if ( !GUILayout.Button( "Test Shake" ) ) return;

			var testInstance = new CumulativeEffectInstanceData
			{
				duration = 1f,
				intensity = 1f,
				intensityCurve = AnimationCurve.EaseInOut( 0f, 1f, 1f, 1f )
			};

			effect.PlayEffectInstance( testInstance );
		}
	}
	#endif
}
