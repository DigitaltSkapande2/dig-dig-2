using UnityEngine;

namespace DigDig2.EffectSystem
{
	public class EffectCleanup : MonoBehaviour
	{
		[SerializeField] private bool killAfterDuration;
		[SerializeField] private float timeUntilKill;

		private void Start( )
		{
			if ( killAfterDuration ) Invoke( nameof( Die ), timeUntilKill );
		}

		private void Die( ) { Destroy( gameObject ); }
	}
}
