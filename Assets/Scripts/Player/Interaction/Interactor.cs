using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2.Player.Interaction {
	public class Interactor : MonoBehaviour {
		public enum InteractionPhase {
			Unknown,
			Began,
			Held,
			Ended
		}

		[Tooltip( "Where the interaction starts from, leave blank to use this GameObject's transform." )]
		[SerializeField] private Transform interactionSource;

		[Tooltip( "The angle you have to be in compared to the interactable to interact with it." )]
		[SerializeField] [Range( 10f, 180f )] private float interactionAngle = 60f;

		[SerializeField] private GameObject interactionPromptBillboardPrefab;

		private readonly List<Interactable> interactablesInRange = new( );

		private Interactable focusedInteractable;
		private GameObject focusedInteractableInteractionPromptBillboard;

		private void Update( ) { UpdateInteractableFocus( ); }

		private void OnTriggerEnter( Collider other ) {
			Interactable interactable = other.GetComponent<Interactable>( );
			if ( !interactable ) return;

			if ( !interactablesInRange.Contains( interactable ) ) interactablesInRange.Add( interactable );
			UpdateInteractableFocus( );
		}

		private void OnTriggerExit( Collider other ) {
			Interactable interactable = other.GetComponent<Interactable>( );
			if ( !interactable ) return;

			if ( interactablesInRange.Contains( interactable ) ) interactablesInRange.Remove( interactable );
			UpdateInteractableFocus( );
		}

		public void UpdateInteractableFocus( ) {
			// Get the interactable with the angle closest to where the player is looking
			Interactable closestInteractable = null;
			float closestInteractableAngle = 180f;
			foreach ( Interactable interactableInRange in interactablesInRange ) {
				float interactableAngle = Vector3.Angle( -interactableInRange.GetInteractableSource( ).forward, interactionSource.transform.forward );

				if ( interactableAngle > interactionAngle ) continue;

				if ( !closestInteractable ) {
					closestInteractable = interactableInRange;
					closestInteractableAngle = interactableAngle;
					continue;
				}

				if ( !( interactableAngle < closestInteractableAngle ) ) continue;

				closestInteractable = interactableInRange;
				closestInteractableAngle = interactableAngle;
			}

			if ( closestInteractable == focusedInteractable ) return;

			if ( focusedInteractable ) focusedInteractable.SetFocus( false );
			if ( focusedInteractableInteractionPromptBillboard ) Destroy( focusedInteractableInteractionPromptBillboard.gameObject );

			focusedInteractable = closestInteractable;

			if ( !focusedInteractable ) return;

			focusedInteractable.SetFocus( true );
			focusedInteractableInteractionPromptBillboard = Instantiate( interactionPromptBillboardPrefab, focusedInteractable.transform.position + new Vector3( 0f, 2f, 0f ), Quaternion.identity );
		}

		public Interactable GetFocusedInteractable( ) => focusedInteractable;

		public bool HasFocusedInteractable( ) => focusedInteractable != null;

		public void SendInteraction( InputActionPhase phase = InputActionPhase.Waiting ) {
			if ( !HasFocusedInteractable( ) ) return;

			InteractionPhase interactionPhase = phase switch {
				InputActionPhase.Started => InteractionPhase.Began,
				InputActionPhase.Performed => InteractionPhase.Held,
				InputActionPhase.Canceled => InteractionPhase.Ended,
				_ => InteractionPhase.Unknown
			};

			focusedInteractable.Interact( interactionPhase );
		}
	}
}
