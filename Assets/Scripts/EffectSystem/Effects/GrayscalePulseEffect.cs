using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DigDig2.EffectSystem.Effects
{
	[RequireComponent( typeof( Volume ) )]
	public class GreyscalePulseEffect : CumulativeEffectBase<CumulativeEffectInstanceData>
	{
		private ColorAdjustments colorAdjustments;
		private ColorAdjustments defaultColorAdjustments;
		private Volume volume;

		private void Start( )
		{
			volume = GetComponent<Volume>( );

			// Ensure the volume has a Vignette effect
			if ( !volume.profile.TryGet( out colorAdjustments ) )
			{
				colorAdjustments = volume.profile.Add<ColorAdjustments>( );
				colorAdjustments.active = true;
			}

			// Store the default vignette settings
			defaultColorAdjustments = ScriptableObject.CreateInstance<ColorAdjustments>( );
			defaultColorAdjustments.saturation = colorAdjustments.saturation;
		}

		internal override void UpdateEffect( float curveValue ) { colorAdjustments.saturation.value = 1 - curveValue; }

		internal override void OnEffectEnd( CumulativeEffectInstanceData effect )
		{
			if ( effectInstances.Count == 0 ) colorAdjustments.saturation.value = defaultColorAdjustments.saturation.value;
		}
	}
}
