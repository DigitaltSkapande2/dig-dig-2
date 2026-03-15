using UnityEngine;

namespace DigDig2.EffectSystem.Effects
{
	public class TimeSlowEffect : CumulativeEffectBase<CumulativeEffectInstanceData>
	{
		internal override void UpdateEffect( float curveValue ) { SetTimeScale( curveValue ); }

		internal override void OnEffectEnd( CumulativeEffectInstanceData effect )
		{
			if ( effectInstances.Count > 1 ) return;
            
			SetTimeScale( 1 );
		}

		private void SetTimeScale( float timeScale )
		{
			Time.timeScale = timeScale;
		}
	}
}
