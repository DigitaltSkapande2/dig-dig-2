using System;

using DigDig2.EffectSystem.Effects;
using DigDig2.Util;

using UnityEngine;

namespace DigDig2.EffectSystem
{
	[RequireComponent( typeof( SpawnPrefabEffect ) )] [RequireComponent( typeof( ScreenShakeEffect ) )] [RequireComponent( typeof( CameraZoomEffect ) )] [RequireComponent( typeof( TimeSlowEffect ) )] [RequireComponent( typeof( VignettePulseEffect ) )] [RequireComponent( typeof( GreyscalePulseEffect ) )]
	public class EffectCore : Singleton<EffectCore>
	{
		[NonSerialized] public CameraZoomEffect cameraZoomEffect;
		[NonSerialized] public GreyscalePulseEffect greyscalePulseEffect;
		[NonSerialized] public ScreenShakeEffect screenShakeEffect;
		[NonSerialized] public SpawnPrefabEffect spawnPrefabEffect;
		[NonSerialized] public TimeSlowEffect timeSlowEffect;
		[NonSerialized] public VignettePulseEffect vignettePulseEffect;

		protected override void Awake( )
		{
			base.Awake( );
			spawnPrefabEffect = GetComponent<SpawnPrefabEffect>( );
			screenShakeEffect = GetComponent<ScreenShakeEffect>( );
			cameraZoomEffect = GetComponent<CameraZoomEffect>( );
			timeSlowEffect = GetComponent<TimeSlowEffect>( );
			vignettePulseEffect = GetComponent<VignettePulseEffect>( );
			greyscalePulseEffect = GetComponent<GreyscalePulseEffect>( );
		}
	}
}
