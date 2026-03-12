using DigDig2.CinemaCamera;
using DigDig2.Game;
using DigDig2.Input;
using DigDig2.Player.Interaction;

using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2.Player
{
	[RequireComponent( typeof( EntityCharacterController ), typeof( SingleplayerCharacterSwitcher ) )]
	public class PlayerCharacterInput : MonoBehaviour
	{
		// Character Switching
		private SingleplayerCharacterSwitcher characterSwitching;

		// Character Controller
		private EntityCharacterController entityCharacterController;
		private bool hasStarted;

		private Vector2 inputMoveVector = Vector2.zero;

		// Interactors
		private Interactor interactor;

		private Camera mainCamera;
		

		public bool InputEnabled { get; private set; }

		private void Awake( )
		{
			entityCharacterController = GetComponent<EntityCharacterController>( );
			characterSwitching = GetComponent<SingleplayerCharacterSwitcher>( );
			interactor = GetComponentInChildren<Interactor>( );
		}

		private void Start( )
		{
			mainCamera = GameCamera.Instance.mainCamera;
			hasStarted = true;
		}

		private void Update( )
		{
			if ( !InputEnabled ) entityCharacterController.inputMoveVector = Vector3.zero;

			if ( mainCamera )
			{
				Vector3 rotatedInputMoveVector = Quaternion.Euler( 0f, GameCamera.Instance.mainCamera.transform.rotation.eulerAngles.y, 0f ) * new Vector3( inputMoveVector.x, 0f, inputMoveVector.y );
				entityCharacterController.inputMoveVector = rotatedInputMoveVector;
			}
			else
				mainCamera = GameCamera.Instance.mainCamera;
		}

		#region Input Action Callbacks

		public void OnMove(InputValue context)
		{
			inputMoveVector = context.Get<Vector2>();
		}

		public void OnInteract(InputValue context)
		{
			//if (interactor) interactor.SendInteraction(context.phase); // TODO: Fix interactions
		}

		public void OnSwitchCharacter(InputValue context)
		{
			if (context.isPressed && characterSwitching != null)
			{
				characterSwitching.SwitchCharacter();
			}
		}

		public void OnDash(InputValue context)
		{
			if (context.isPressed)
			{
				entityCharacterController.Dash();
			}
		}

		public void OnPause(InputValue context)
		{
			if (context.isPressed)
			{
				GameManager.Instance.Pause();
			}
		}

		#endregion
	}
}
