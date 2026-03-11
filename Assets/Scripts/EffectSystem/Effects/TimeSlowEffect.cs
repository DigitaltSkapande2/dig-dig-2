using UnityEngine;

namespace DigDig2.EffectSystem.Effects {
	public class TimeSlowEffect : CumulativeEffectBase<CumulativeEffectInstanceData> {
		internal override void UpdateEffect( float curveValue ) { SetTimeScale( curveValue ); }

		internal override void OnEffectEnd( CumulativeEffectInstanceData effect ) {
			Debug.Log( $"SKIBIDI TOILET {effectInstances.Count}" );
			if ( effectInstances.Count > 1 ) return;

			Debug.Log( "WIZZZ" );
			SetTimeScale( 1 );
		}

		private void SetTimeScale( float timeScale ) {
			Debug.Log( "TIMESCALE: " + timeScale );
			Time.timeScale = timeScale;
		}
	}
}
