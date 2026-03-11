using UnityEngine;
using UnityEngine.Splines;

namespace DigDig2
{
	public class Movable : MonoBehaviour
	{
		[SerializeField] private float moveTime;
		[SerializeField] private AnimationCurve speedCurve;

		[SerializeField] private SplineContainer path;

		[SerializeField] private bool reusable;
		private int splineIndex;

		private Vector3 startPos;
		private float time;

		private bool triggered;

		private void Update( )
		{
			if ( !triggered ) return;

			time += Time.deltaTime;
			float value = time / moveTime;

			float lerp = speedCurve.Evaluate( value );

			transform.position = path.EvaluatePosition( splineIndex, lerp );

			if ( !( value >= 1 ) || !reusable ) return;

			time = 0;
			triggered = false;

			splineIndex++;
			if ( splineIndex >= path.Splines.Count ) splineIndex = 0;
		}

		public void Trigger( )
		{
			startPos = transform.position;
			triggered = true;
		}
	}
}
