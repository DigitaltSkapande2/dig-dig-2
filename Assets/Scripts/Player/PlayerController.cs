using System.Collections.Generic;
using DigDig2.CinemaCamera;
using DigDig2.Combat;
using DigDig2.Game;
using DigDig2.Input;
using DigDig2.Player.Interaction;

using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2.Player
{
	[RequireComponent( typeof( EntityCharacterController ), typeof( SingleplayerCharacterSwitcher ) )]
	public class PlayerController : MonoBehaviour
	{
		public Vector2 inputMoveVector = Vector2.zero;
        
        public CharacterType characterType;
        
        public int inputPlayerIndex = -1;
        public List<InputDevice> inputDevice;
        public bool IsAlive => health.IsAlive;

		// Interactors
		private Interactor interactor;
		private Camera mainCamera;
        
        // Character Controller
        private EntityCharacterController entityCharacterController;
        public Health health;

		private void Awake( )
		{
			entityCharacterController = GetComponent<EntityCharacterController>( );
            health = GetComponent<Health>();
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
        
        #region setup methods

        public void SetInputPlayerID(int newid)
        {
            inputPlayerIndex = newid;
            inputDevice = InputManager.Instance.GetInputPlayersDevices(newid);
            foreach (var inputModule in GetComponents<InputModule>())
            {
                inputModule.AllowedInputPlayerIndex = newid;
            }
        }
        
        #endregion

		#region Input Action Callbacks

		private void OnInputGameMove( InputInfo inputInfo )
		{
			inputMoveVector = inputInfo.context.ReadValue<Vector2>( );
		}

		private void OnInputGameInteract( InputInfo inputInfo )
		{
			if ( interactor ) interactor.SendInteraction( inputInfo.context.phase );
		}

		private void OnInputGameDash( InputInfo inputInfo )
		{
			if ( inputInfo.context.performed ) entityCharacterController.Dash( );
		}

		#endregion
	}
}
