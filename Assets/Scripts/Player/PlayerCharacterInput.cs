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

		private Vector2 inputMoveVector = Vector2.zero;

		// Interactors
		private Interactor interactor;

		private Camera mainCamera;

		private void Awake( )
		{
			entityCharacterController = GetComponent<EntityCharacterController>( );
			characterSwitching = GetComponent<SingleplayerCharacterSwitcher>( );
			interactor = GetComponentInChildren<Interactor>( );
		}

		private void Start( )
		{
			mainCamera = GameCamera.Instance.mainCamera;
		}

		private void Update( )
		{
			if ( GameManager.Instance.Paused ) entityCharacterController.inputMoveVector = Vector3.zero;

			if ( mainCamera )
			{
				Vector3 rotatedInputMoveVector = Quaternion.Euler( 0f, GameCamera.Instance.mainCamera.transform.rotation.eulerAngles.y, 0f ) * new Vector3( inputMoveVector.x, 0f, inputMoveVector.y );
				entityCharacterController.inputMoveVector = rotatedInputMoveVector;
			}
			else
				mainCamera = GameCamera.Instance.mainCamera;
		}

		#region Input Action Callbacks

		private void OnInputGameMove( InputInfo inputInfo )
		{
			inputMoveVector = inputInfo.context.ReadValue<Vector2>( );
		}

		private void OnInputGameInteract( InputInfo inputInfo )
		{
			if ( interactor ) interactor.SendInteraction( inputInfo.context.phase );
		}

		private void OnInputGameSwitchCharacter( InputInfo inputInfo )
		{
			if ( inputInfo.context.performed && characterSwitching ) characterSwitching.SwitchCharacter( );
		}

		private void OnInputGameDash( InputInfo inputInfo )
		{
			if ( inputInfo.context.performed ) entityCharacterController.Dash( );
		}

		private void OnInputGamePause( InputInfo inputInfo )
		{
			if ( inputInfo.context.performed ) GameManager.Instance.Pause( );
		}

		#endregion
	}
}
