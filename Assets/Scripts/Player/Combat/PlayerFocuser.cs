using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using DigDig2.Input;
using DigDig2.Combat;
using DigDig2.CinemaCamera;
using DigDig2.Game;
using DigDig2.Debugging;
using UnityEngine.InputSystem;

namespace DigDig2.Player.Combat
{
    [RequireComponent(typeof(EntityCharacterController))]
    public class PlayerFocuser : MonoBehaviour
    {
        [Header("Focusing")]
        [SerializeField] private string[] focusEnemyGroupPriorityList;
        [Tooltip( "The radius to scan for enemies at when focusing" )]
        [SerializeField] private float focusScanRadius = 10;
        [Tooltip( "The distance in Y (elevation) for focusing, prevents focusing on stuff under water etc" )]

        [SerializeField] private float yDifferenceTolerance = 1f;

        [Header("Camera Focus Effect")]
        [Tooltip( "how much to lerp towards the focus target" )]
        [SerializeField] private float cameraFocusFollowFactor;

        [SerializeField] private float cameraFocusFrustumSize;

        private Attackable currentlyFocusedAttackable;
        private bool isFocusing;
        private bool isTargeting;
        private float targetingDirection;
        
        [NonSerialized] public UnityEvent<Vector3> focusPositionUpdated = new();
        [NonSerialized] public UnityEvent<bool> isFocusingStateChanged = new();
        
        private EntityCharacterController entityController;
        private GameCamera gameCamera;
        private CameraEffector cameraEffector;
        
        #region UnityCallbacks

        private void Start()
        {
            if (!TryGetComponent<EntityCharacterController>(out entityController)) BetterDebug.Log("EntityCharacterController NOT FOUND.");
            gameCamera = GameCamera.Instance;

            cameraEffector = new GameObject().gameObject.AddComponent<CameraEffector>();
        }

        private void Update()
        {
            UpdateFocusing();
        }

        private void OnDestroy()
        {
            EndFocusing();
            Destroy(cameraEffector.gameObject, 10f);
        }

        #endregion
        #region Input Callbacks
        
        private void OnInputCombatFocus( InputInfo inputInfo )
        {
            isFocusing = !inputInfo.context.canceled;

            if (inputInfo.context.started)
            {
                BeginFocusing();
            }
            else if (inputInfo.context.canceled)
            {
                EndFocusing();
            }
        }

        private void OnInputCombatFocusTarget(InputInfo inputInfo)
        {
            if (!isFocusing) return;
            
            Vector2 vector = inputInfo.context.ReadValue<Vector2>();
            float stickAngle = Mathf.Atan2(vector.x, vector.y) * Mathf.Rad2Deg;
            targetingDirection = stickAngle + gameCamera.mainCamera.transform.eulerAngles.y;
            
            SetTargeting(!inputInfo.context.canceled);
        }

        private void OnInputCombatFocusTargetMouse(InputInfo inputInfo)
        {
            if (!isFocusing) return;
            Debug.Log(inputInfo.context.ReadValue<Vector2>());
            
            Vector2 mouseScreenPos = inputInfo.context.ReadValue<Vector2>();
            Vector2 playerScreenPos = Camera.main.WorldToScreenPoint(transform.position);

            Vector2 screenDir = (mouseScreenPos - playerScreenPos).normalized;
            
            float stickAngle = Mathf.Atan2(screenDir.x, screenDir.y) * Mathf.Rad2Deg;
            targetingDirection = stickAngle + Camera.main.transform.eulerAngles.y;
            
            
            SetTargeting(!inputInfo.context.canceled);
        }

        private void SetTargeting(bool active)
        {
            isTargeting = active;
            UpdateFocusTarget();
        }

        #endregion
        
        #region Enemy Focusing

		public List<Attackable> GetEnemiesInRadius( float radius )
        {
            float myYPos = transform.position.y;
			Collider[ ] scannedColliders = Physics.OverlapSphere( transform.position, radius );
            Dictionary<string, List<Attackable>> scannedenemyGroupedAttackables = new();
			foreach ( Collider scannedCollider in scannedColliders )
			{
                
				if ( !scannedCollider.TryGetComponent( out Attackable enemyAttackable ) ) continue;
                if (Mathf.Abs(scannedCollider.transform.position.y - myYPos) > yDifferenceTolerance) continue;

                if (scannedenemyGroupedAttackables.ContainsKey(enemyAttackable.Group))
                {
                    scannedenemyGroupedAttackables[enemyAttackable.Group].Add(enemyAttackable);
                }
                else
                {
					scannedenemyGroupedAttackables.Add(enemyAttackable.Group, new List<Attackable>() { enemyAttackable });
                }
                
			}

            for (int i = 0; i < focusEnemyGroupPriorityList.Length; i++)
			{
                if (scannedenemyGroupedAttackables.ContainsKey(focusEnemyGroupPriorityList[i]))
				{
                    return scannedenemyGroupedAttackables[focusEnemyGroupPriorityList[i]];
                }
            }

            return new();
        }

		public Attackable GetClosestEnemyInRadius( float radius )
		{
			Attackable closestEnemy = null;
			float closestEnemyAngle = 180;
			foreach ( Attackable enemy in GetEnemiesInRadius( radius ) )
			{
				
                if (!isTargeting) targetingDirection = entityController.TargetLookRotation;
                Vector3 toEnemy = enemy.transform.position - transform.position;
                float enemyAngle = Mathf.Atan2(toEnemy.x, toEnemy.z) * Mathf.Rad2Deg;
                float delta = Mathf.Abs(Mathf.DeltaAngle(enemyAngle, targetingDirection));
                if (closestEnemyAngle <= delta) continue;

				closestEnemy = enemy;
                closestEnemyAngle = delta;
            }
            //BetterDebug.Log($"name: {closestEnemy.name}, {gay}, {entityController.TargetLookRotation}");

			return closestEnemy;
		}

		public void BeginFocusing( )
        {
            cameraEffector.targetFrustumSize = cameraFocusFrustumSize;
            UpdateFocusTarget();
        }

		public void EndFocusing( )
        {
            cameraEffector.targetFrustumSize = 0f;
            cameraEffector.targetPosition = Vector3.zero;
            
            currentlyFocusedAttackable = null;
			if ( entityController ) entityController.SetAutomaticLookRotationLock( false );
            
            isFocusingStateChanged.Invoke(false);
		}

        public void UpdateFocusTarget()
        {
            Attackable closestEnemy = GetClosestEnemyInRadius( focusScanRadius );
            if (closestEnemy && IsAttackableVisibleOnScreen(closestEnemy))
            {
                currentlyFocusedAttackable = closestEnemy;
                isFocusingStateChanged.Invoke(true);
            }
            else
            {
                EndFocusing();
            }
        }

		private void UpdateFocusing( )
		{
            if (!isFocusing) return;
            bool hasFocusedEnemy = (bool)currentlyFocusedAttackable;
            
            // If focused enemy not on screen EndFocus()
			if ( !hasFocusedEnemy || !IsAttackableVisibleOnScreen( currentlyFocusedAttackable ) )
            {
                UpdateFocusTarget();
                if (!currentlyFocusedAttackable) return;
            } 
            
            // Camera effector
            cameraEffector.targetPosition = (currentlyFocusedAttackable.transform.position - gameCamera.transform.position) *
                                            cameraFocusFollowFactor;

			// Set Screen Marker Position
			if ( hasFocusedEnemy && GameManager.Instance.PlayerCharacterObjects.Contains(gameObject))
			{
                Vector3 enemyPosition = currentlyFocusedAttackable!.transform.position;
                
				focusPositionUpdated.Invoke(enemyPosition);
			}

			// EntityCharacter Controller rotation
            if (entityController)
            {
                entityController.SetAutomaticLookRotationLock( hasFocusedEnemy );
                if ( hasFocusedEnemy ) entityController.LookTowards( currentlyFocusedAttackable!.transform.position );
            }
		}

		public bool IsAttackableVisibleOnScreen( Attackable attackable )
		{
			if ( !attackable.TryGetComponent( out Collider attackableCollider ) ) return false;

			Plane[ ] cameraFrustumPlanes = GeometryUtility.CalculateFrustumPlanes( GameCamera.Instance.mainCamera );
			return GeometryUtility.TestPlanesAABB( cameraFrustumPlanes, attackableCollider.bounds );
		}

		#endregion
    }
}