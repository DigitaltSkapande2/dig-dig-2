using UnityEngine;
using UnityEngine.Events;

namespace DigDig2.Game {
	[RequireComponent( typeof( Collider ) )]
	public class GameplayTrigger : MonoBehaviour {
		[SerializeField] private UnityEvent triggerEvent;

		private void OnTriggerEnter( Collider other ) {
			if ( other.CompareTag( "Player" ) ) triggerEvent.Invoke( );
		}
	}
}
