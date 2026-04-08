using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;

namespace DigDig2
{
	public class Movable : MonoBehaviour
	{
		[SerializeField] private float moveTime;
		[SerializeField] private AnimationCurve positionCurve;

		[SerializeField] private SplineContainer path;

		[SerializeField] private bool reusable;
        [SerializeField] private UnityEvent
		private int splineIndex;
        
		private float startTime;

		private bool triggered;

        private UniTask currentlyMovingOperation;

		public void Trigger( )
		{
            if (triggered) return;
            startTime = Time.time;
            MovePlatformToTarget().Forget();
        }

        private async UniTask MovePlatformToTarget()
        {
            triggered = true;

            float value = 0f;
            
            while (value < 1f)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
                value = (Time.time - startTime) / moveTime;

                float pos = positionCurve.Evaluate( value );

                transform.position = path.EvaluatePosition( splineIndex, pos );
            }
            
            if ( reusable )
            {
                splineIndex++;
                if ( splineIndex >= path.Splines.Count ) splineIndex = 0;
            }
            
            triggered = false;
        }
	}
}
