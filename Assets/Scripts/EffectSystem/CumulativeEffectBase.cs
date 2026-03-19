using System;

using UnityEngine;

namespace DigDig2.EffectSystem
{
	[Serializable]
	public class CumulativeEffectInstanceData : ICloneable
	{
		public AnimationCurve intensityCurve = AnimationCurve.EaseInOut( 0f, 1f, 1f, 0f );
		public float duration = 0.4f;

		public float intensity = 1f;

		[NonSerialized] internal float durationPassed;

		public virtual object Clone( ) => MemberwiseClone( );
	}

	/// <summary>Cumulative, meaning the effect stacks if called multiple times before finishing</summary>
	public class CumulativeEffectBase<T> : Effect<T> where T : CumulativeEffectInstanceData
	{
		internal new void Update( )
		{
			float cumulativeCurveValue = 0f;
            
			for ( int i = effectInstances.Count - 1; i >= 0; i-- )
			{
				T effect = effectInstances[ i ];
				if ( effect == null )
				{
					effectInstances.RemoveAt( i );
					continue;
				}
                
				effect.durationPassed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

				// expired?
				if ( effect.durationPassed > effect.duration )
				{
					OnEffectEnd( effect );
					effectInstances.RemoveAt( i );
					continue;
				}

				// evaluate curve
				float normalized = Mathf.Clamp01( effect.durationPassed / Mathf.Max( float.Epsilon, effect.duration ) );
				float curveValue = effect.intensityCurve.Evaluate( normalized );
				cumulativeCurveValue += curveValue * effect.intensity;
			}

			if ( effectInstances.Count > 0 ) UpdateEffect( cumulativeCurveValue );
			if ( effectInstances.Count > 0 ) UpdateEffect( cumulativeCurveValue );
		}

		internal override void OnEffectStart( T effect )
		{
			if ( effect == null ) return;

			effect.durationPassed = 0f;
		}

		internal virtual void UpdateEffect( float cumulativeValue )
		{
			
		}
	}
}
