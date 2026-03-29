using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using DigDig2.CinemaCamera;
using UnityEngine;
using DigDig2.Combat;
using DigDig2.Debugging;
using DigDig2.EffectSystem;
using DigDig2.Input;
using DigDig2.Game;
using DigDig2.Entity;


namespace DigDig2.Player
{
    public class PlayerCharacterController : MonoBehaviour
    {
        [SerializeField] private EffectPlayer singleplayerDeathEffectPlayer;
        [SerializeField] private EffectPlayer multiplayerDeathEffectPlayer;
        [SerializeField] private float disolveWeight = 3f;
        [Header("Dissolve Visuals")]
        [SerializeField] private SkinnedMeshRenderer[ ] targetMeshRenderers;
        [SerializeField] private string dissolveFloatName = "_DissolveAmount";

        private Health health;
        private Attacker attacker;
        private GameManager gameManager;
        private Camera mainCamera;
        private EntityCharacterController entityCharacterController;

        public bool shouldStartDissolved;
        public Vector2 inputMoveVector = Vector2.zero;

        private bool isActive = true;

        private void Awake()
        {
            health = GetComponent<Health>();
            attacker = GetComponent<Attacker>( );
            entityCharacterController = GetComponent<EntityCharacterController>();
            
            health.death.AddListener(OnDeath);
        }

        private void Start()
        {
            gameManager = GameManager.Instance;
            mainCamera = GameCamera.Instance?.mainCamera ?? Camera.main;
            
            if (shouldStartDissolved) DissolveRoutine(1, 0).Forget();
            isActive = true;
        }

        private void Update()
        {
            UpdateCharacterMovement();
        }

        #region Dissolve & Death

        public async void OnDeath(GameObject _)
        {
            entityCharacterController.Frozen = true;
            if (GameManager.Instance.IsMultiplayer)
            {
                multiplayerDeathEffectPlayer?.Play( transform.position, Quaternion.identity, Vector3.one );
                GameManager.Instance.RegisterCharacterDeath(gameObject);
            }
            else
            {
                await Disappear(true);
                GameManager.Instance.RegisterCharacterDeath(gameObject); 
            }
        }

        public async UniTask Disappear( bool destroyAfter )
        {
            isActive = false;
            await DissolveRoutine(0, 1);
            if (destroyAfter) Destroy(gameObject);
        }

        private async UniTask DissolveRoutine(float startVal, float targetVal)
        {
            float newDissolveAmount = startVal;
            while (Mathf.Abs(newDissolveAmount - targetVal) > 0.01f )
            {
                newDissolveAmount = Mathf.Lerp( newDissolveAmount, targetVal, 1f - Mathf.Exp(-disolveWeight * Time.deltaTime) );
                foreach ( SkinnedMeshRenderer targetMeshRenderer in targetMeshRenderers )
                {
                    Material[] mats = targetMeshRenderer.materials;
                    foreach ( Material mat in mats )
                    {
                        mat.SetFloat( dissolveFloatName, newDissolveAmount );
                    }
                    targetMeshRenderer.materials = mats;
                }
                await UniTask.Yield( PlayerLoopTiming.Update );
            }
        }

        #endregion
        #region Movement
        
        private void UpdateCharacterMovement()
        {
            if (gameManager.Paused || !isActive)
            {
                entityCharacterController.inputMoveVector = Vector3.zero;
                return;
            }

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
            if ( GameManager.Instance.Paused || !isActive ) return;
			
            if ( inputInfo.context.performed )
                attacker.RequestAttackStart( 0 );
            else
                attacker.RequestAttackEnd( true );
        }

        private void OnInputCombatAttack2( InputInfo inputInfo )
        {
            if ( GameManager.Instance.Paused || !isActive ) return;
			
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
            if ( GameManager.Instance.Paused || !isActive ) return;
            if ( inputInfo.context.started ) entityCharacterController.Dash( );
        }

        #endregion
    }
}