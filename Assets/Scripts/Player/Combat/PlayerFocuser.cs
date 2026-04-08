using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DigDig2.Input;
using DigDig2.Combat;
using DigDig2.CinemaCamera;
using DigDig2.Debugging;
using DigDig2.Entity;
using UnityEngine.UIElements;

namespace DigDig2.Player.Combat
{
    public class PlayerFocuser : MonoBehaviour
    {
        [SerializeField] private PlayerController targetPlayer;
        [Header("Focusing")]
        [SerializeField] private float focusTargetIndicatorRotationSpeed = 10f;
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
        public bool isFocusing;
        private bool isTargeting;
        private Vector3 targetingDirection = Vector3.forward;
        
        private EntityCharacterController entityController => targetPlayer.entityController;
        private GameCamera gameCamera;
        private CameraEffector cameraEffector;

        private UIDocument uiDocument;
        private VisualElement singlePlayerFocusIndicator;
        private VisualElement singlePlayerFocusTargetIndicatorImage;

        private int currentlyFocusedEnemyGroupIndex;
        
        
        // Scan routine - routinely scan for attackables
        private CancellationTokenSource _scanCancelationTokenSource;
        public Collider[] _colliderBuffer = new Collider[64]; // Used to store colliders during routine scans,
                                                                        // Why: Rider complained on me that i should use NonAloc, and someone online said
                                                                        // using just OverlapSphere every frame is heavy on the allocation stuff, so
                                                                        // defining one array then using that is peak
        public List<Attackable> currentTargetableEnemies = new();

        #region UnityCallbacks

        private void OnEnable()
        {
            ScanLoop(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private void Start()
        {
            uiDocument = GetComponent<UIDocument>();
            gameCamera = GameCamera.Instance;

            singlePlayerFocusIndicator = uiDocument.rootVisualElement.Query("focusTargetIndicator");
            singlePlayerFocusTargetIndicatorImage = singlePlayerFocusIndicator.Query("image");

            cameraEffector = gameObject.AddComponent<CameraEffector>();
        }

        private async UniTask ScanLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (isFocusing)
                {
                    ScanForAttackables();
                    await UniTask.Delay(200, cancellationToken: ct);
                }

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
                // else
                // {
                //     BetterDebug.Log("AOFKG+ekg");
                //     await UniTask.WaitUntil(() => isFocusing, cancellationToken: ct);
                // }
            }
        }

        private void Update()
        {
            UpdateFocusing();
            RotateFocusIndicatorImage(singlePlayerFocusIndicator, singlePlayerFocusTargetIndicatorImage);
            
            if ( currentlyFocusedAttackable )
            {
                Vector3 enemyPosition = currentlyFocusedAttackable!.transform.position;
                UpdateFocusIndicatorPosition(enemyPosition);
            }
            
            if (entityController)
            {
                entityController.SetAutomaticLookRotationLock( currentlyFocusedAttackable );
                if ( currentlyFocusedAttackable ) entityController.LookTowards( currentlyFocusedAttackable!.transform.position );
            }
        }

        private void OnDestroy()
        {
            EndFocusing();
        }

        #endregion
        
        #region Input Callbacks
        
        private void OnInputCombatFocus( InputInfo inputInfo )
        {
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

            Transform cam = gameCamera.mainCamera.transform;
            Vector3 camRight   = Vector3.ProjectOnPlane(cam.right,   Vector3.up).normalized;
            Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;

            targetingDirection = (camRight * vector.x + camForward * vector.y).normalized;

            SetTargeting(!inputInfo.context.canceled);
        }

        private void OnInputCombatFocusTargetMouse(InputInfo inputInfo)
        {
            if (!isFocusing) return;

            Vector2 mouseScreenPos = inputInfo.context.ReadValue<Vector2>();
            Vector2 playerScreenPos = gameCamera.mainCamera.WorldToScreenPoint(
                targetPlayer.characterObject.transform.position
            );
            Vector2 screenDir = (mouseScreenPos - playerScreenPos).normalized;
            
            Vector3 camRight   = Vector3.ProjectOnPlane(gameCamera.mainCamera.transform.right,   Vector3.up).normalized;
            Vector3 camForward = Vector3.ProjectOnPlane(gameCamera.mainCamera.transform.forward, Vector3.up).normalized;

            targetingDirection = (camRight * screenDir.x + camForward * screenDir.y).normalized;

            SetTargeting(!inputInfo.context.canceled);
        }

        private void SetTargeting(bool active)
        {
            isTargeting = active;
            ReCalculateFocusTarget();
        }

        #endregion
        
        #region Enemy Focusing
        
        public void BeginFocusing( )
        {
            ScanForAttackables();
            isFocusing = true;
            cameraEffector.targetFrustumSize = cameraFocusFrustumSize;
            ReCalculateFocusTarget();
        }

        public void EndFocusing( )
        {
            isFocusing = false;
            cameraEffector.targetFrustumSize = 0f;
            cameraEffector.targetPosition = Vector3.zero;
            
            currentlyFocusedAttackable = null;
            currentlyFocusedEnemyGroupIndex = -1;
            if ( entityController ) entityController.SetAutomaticLookRotationLock( false );
            
            SetFocusIndicatorActive(false);
        }

        private void UpdateFocusing( )
        {
            if (!isFocusing) return;
            
            if ( !currentlyFocusedAttackable || !IsAttackableVisibleOnScreen( currentlyFocusedAttackable ) )
            {
                ReCalculateFocusTarget();
                if ( !currentlyFocusedAttackable ) EndFocusing();
            } 
        }
        
        public void ReCalculateFocusTarget()
        {
            if (TryGetClosestEnemy(out var attackable) && IsAttackableVisibleOnScreen(attackable)) 
            {
                currentlyFocusedAttackable = attackable;
                SetFocusIndicatorActive(true);
            }
            else
            {
                EndFocusing();
            }
        }
        
        public bool TryGetClosestEnemy(out Attackable closestEnemy)
        {
            closestEnemy = null;
            float bestDot = -1f;

            foreach (Attackable enemy in currentTargetableEnemies)
            {
                if (!enemy) continue;

                Vector3 toEnemy = (enemy.transform.position - targetPlayer.characterObject.transform.position);
                toEnemy.y = 0f;
                toEnemy.Normalize();

                Vector3 referenceDir = isTargeting ? targetingDirection : Vector3.ProjectOnPlane(
                        Quaternion.Euler(0, entityController.TargetLookRotation, 0) * Vector3.forward,
                        Vector3.up
                    ).normalized;

                float dot = Vector3.Dot(toEnemy, referenceDir);
                if (dot <= bestDot) continue;

                closestEnemy = enemy;
                bestDot = dot;
            }

            return closestEnemy != null;
        }

        private void ScanForAttackables()
        {
            BetterDebug.Log("Attackable Scan :>");
            Vector3 myPos = targetPlayer.characterObject.transform.position;
            int colliderCount = Physics.OverlapSphereNonAlloc( myPos, focusScanRadius, _colliderBuffer );
            
            // -- Get and Sort Attackables
            var priorityIndexSortedAttackables = new Dictionary<int, List<Attackable>>();
            
            for (int i = 0; i < colliderCount; i++)
            {
                if (!_colliderBuffer[i].TryGetComponent(out Attackable attackable)) continue;
                if (Mathf.Abs(_colliderBuffer[i].transform.position.y - myPos.y) > yDifferenceTolerance) continue;
                
                int priorityIndex = Array.IndexOf(focusEnemyGroupPriorityList, attackable.Group);
                if (priorityIndex < 0) continue;

                // Add to list, if no list, make new list :>
                if (!priorityIndexSortedAttackables.ContainsKey(priorityIndex))
                {
                    priorityIndexSortedAttackables[priorityIndex] = new();
                }
                priorityIndexSortedAttackables[priorityIndex].Add(attackable);
            }
            
            // -- Update list
            int highestPriorityIndex = priorityIndexSortedAttackables.Keys.Min();
            
            currentTargetableEnemies.Clear();
            currentTargetableEnemies.AddRange(priorityIndexSortedAttackables[highestPriorityIndex]);
            if (currentlyFocusedEnemyGroupIndex != highestPriorityIndex)
            {
                currentlyFocusedEnemyGroupIndex = highestPriorityIndex;
                ReCalculateFocusTarget();
            }
        }

		public static bool IsAttackableVisibleOnScreen( Attackable attackable )
		{
			if ( !attackable.TryGetComponent( out Collider attackableCollider ) ) return false;

			Plane[ ] cameraFrustumPlanes = GeometryUtility.CalculateFrustumPlanes( GameCamera.Instance.mainCamera );
			return GeometryUtility.TestPlanesAABB( cameraFrustumPlanes, attackableCollider.bounds );
		}

		#endregion
        #region Camera Effector

        private void UpdateCameraEffector()
        {
            cameraEffector.targetPosition = (currentlyFocusedAttackable.transform.position - gameCamera.transform.position) * cameraFocusFollowFactor;
        }
        
        #endregion

        #region UIGraphics

        private void RotateFocusIndicatorImage(VisualElement indicator, VisualElement indicatorImage)
        {
            indicatorImage.style.rotate = new(
                new Rotate(
                    indicatorImage.resolvedStyle.rotate.angle.value +
                    Time.deltaTime * focusTargetIndicatorRotationSpeed / indicator.resolvedStyle.scale.value.x
                )
            );
        }
        
        
        public void UpdateFocusIndicatorPosition(Vector3 worldPosition )
        {
            //BetterDebug.Log($"UpdateFocusIndicatorPosition {worldPosition}");
            Vector2 screenPosition = RuntimePanelUtils.CameraTransformWorldToPanel( uiDocument.rootVisualElement.panel, worldPosition, gameCamera.mainCamera );
            singlePlayerFocusIndicator.style.translate = new( new Translate( screenPosition.x, screenPosition.y ) );
        }

        private void SetFocusIndicatorActive(bool active)
        {
            //BetterDebug.Log($"SetFocusIndicatorActive {active}");
            singlePlayerFocusIndicator.style.display = new( active ? DisplayStyle.Flex : DisplayStyle.None );
            singlePlayerFocusIndicator.style.opacity = new( active ? 1f : 0f );
            singlePlayerFocusIndicator.style.scale = new( new Scale( active ? new( 1f, 1f ) : new Vector2( 2f, 2f ) ) );

        }

        #endregion
    }
}