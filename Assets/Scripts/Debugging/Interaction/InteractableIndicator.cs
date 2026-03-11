using DigDig2.Player.Interaction;

using UnityEngine;

namespace DigDig2.Debugging.Interaction {
	public class InteractableIndicator : MonoBehaviour {
		public void Interact( Interactor.InteractionPhase phase ) {
			MeshRenderer meshRenderer = GetComponent<MeshRenderer>( );

			meshRenderer.material.color = phase switch {
				Interactor.InteractionPhase.Unknown => new( Random.Range( 0f, 1f ), Random.Range( 0f, 1f ), Random.Range( 0f, 1f ) ),
				Interactor.InteractionPhase.Began => new( 0f, 1f, 0f ),
				Interactor.InteractionPhase.Held => new( 0f, 1f, 1f ),
				Interactor.InteractionPhase.Ended => new( 1f, 0f, 0f ),
				_ => meshRenderer.material.color
			};
		}
	}
}
