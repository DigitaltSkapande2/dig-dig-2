using System;
using DigDig2.CinemaCamera;
using UnityEngine;
using DigDig2.Combat;
using DigDig2.Input;
using DigDig2.Game;
using DigDig2.Entity;


namespace DigDig2.Player
{
    public class PlayerCharacterInput : MonoBehaviour
    {
        private Attacker attacker;
        private GameManager gameManager;
        private Camera mainCamera;
        private EntityCharacterController entityCharacterController;
        
        public Vector2 inputMoveVector = Vector2.zero;

        private void Awake()
        {
            attacker = GetComponent<Attacker>( );
            entityCharacterController = GetComponent<EntityCharacterController>();
        }

        private void Start()
        {
            gameManager = GameManager.Instance;
            mainCamera = GameCamera.Instance?.mainCamera ?? Camera.main;
        }

        private void Update()
        {
            UpdateCharacterMovement();
        }

        #region Movement
        
        private void UpdateCharacterMovement()
        {
            if ( gameManager.Paused ) entityCharacterController.inputMoveVector = Vector3.zero;

            if ( mainCamera )
            {
                Vector3 rotatedInputMoveVector = Quaternion.Euler( 0f, mainCamera.transform.rotation.eulerAngles.y, 0f ) * new Vector3( inputMoveVector.x, 0f, inputMoveVector.y );
                entityCharacterController.inputMoveVector = rotatedInputMoveVector;
            }
            else if (GameCamera.Instance is { } cam)
            {
                mainCamera = cam.mainCamera;
            }
        }
        
        #endregion
        #region Input Action Callbacks

        private void OnInputCombatAttack1( InputInfo inputInfo )
        {
            if ( GameManager.Instance.Paused ) return;
			
            if ( inputInfo.context.performed )
                attacker.RequestAttackStart( 0 );
            else
                attacker.RequestAttackEnd( true );
        }

        private void OnInputCombatAttack2( InputInfo inputInfo )
        {
            if ( GameManager.Instance.Paused ) return;
			
            if ( inputInfo.context.performed )
                attacker.RequestAttackStart( 1 );
            else
                attacker.RequestAttackEnd( );
        }
        
        private void OnInputGameMove( InputInfo inputInfo )
        {
            inputMoveVector = inputInfo.context.ReadValue<Vector2>( );
        }

        private void OnInputGameInteract( InputInfo inputInfo )
        {
            //if ( interactor ) interactor.SendInteraction( inputInfo.context.phase ); //TODO FIX INTERACTOR
        }

        private void OnInputGameDash( InputInfo inputInfo )
        {
            if ( inputInfo.context.started ) entityCharacterController.Dash( );
        }

        #endregion
    }
}