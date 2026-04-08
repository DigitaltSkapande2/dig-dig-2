using DigDig2.EffectSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

namespace DigDig2
{
	public class Movable : MonoBehaviour
	{
		[SerializeField] private float moveTime;
		[SerializeField] private AnimationCurve positionCurve;

		[SerializeField] private SplineContainer path;
		[SerializeField] private EffectPlayer effect;

		[SerializeField] private bool reusable;
		[SerializeField] private UnityEvent triggerEvent;
		private int splineIndex;

		private float time;
		private bool triggered;

		private void Update( )
		{
			if ( !triggered ) return;

			time += Time.deltaTime;
			float value = time / moveTime;

			float pos = positionCurve.Evaluate( value );

			transform.position = path.EvaluatePosition( splineIndex, pos );

			if ( !( value >= 1 ) || !reusable ) return;

			time = 0;
			triggered = false;

			splineIndex++;
			if ( splineIndex >= path.Splines.Count ) splineIndex = 0;
		}

		public void Trigger( )
		{
			if (triggered) return;

			effect?.Play();
			triggerEvent.Invoke();
			triggered = true;
		}
	}
}
