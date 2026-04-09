using UnityEngine;
using UnityEngine.Events;

namespace DigDig2.Game
{
	[RequireComponent( typeof( Collider ) )]
	public class GameplayTrigger : MonoBehaviour
	{
		public UnityEvent triggerEvent;

		private void OnTriggerEnter( Collider other )
		{
			if ( other.CompareTag( "Player" ) ) triggerEvent.Invoke( );
		}
	}
}
