using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DigDig2.Combat;
using DigDig2.Debugging;
using DigDig2.Debugging.Menu;

using UnityEngine;

namespace DigDig2.Entity
{
	[Debug( DebugMenuToggleable.NonToggleable )] [RequireComponent( typeof( CharacterController ) )]
	public class EntityCharacterController : MonoBehaviour
	{
		private const string IDLE_ANIMATION_NAME = "Idle";
		private const string SPRINT_ANIMATION_NAME = "Sprint";
		private const string DASH_ANIMATION_NAME = "Dash";
		private const string SWORD_ANIMATION_NAME_PREFIX = "Sword";
		private static readonly int movementSpeed = Animator.StringToHash( "MovementSpeed" );

		[Tooltip( "Freezes the entity (stops running ´CharacterController.Move()´) and disables the CharacterController component. Allows you to move the entity by setting their transform. Use EntityCharacterController.Teleport() for teleporting instead." )]
		[SerializeField] private bool frozen;

		[Header( "Movement" )]
		[Tooltip( "To change the actual gravity value go to Project Settings > Physics > Settings > Gravity, tough this effects all physics." )]
		[SerializeField] private float gravityScale = 1f;

		[Space( 20 )]
		[SerializeField] private LayerMask groundLayers;

		[Space( 20 )]
		[Tooltip( "The max speed the entity can walk at." )]
		[SerializeField] [DebugSerialized] private float moveSpeed = 5f;

		[Tooltip( "The direction the entity is currently moving." )]
		[SerializeField] public Vector3 inputMoveVector = Vector3.zero;

		[Tooltip( "The move acceleration and decelleration speed, higher is faster." )]
		[SerializeField] private float moveInputVectorLerpSpeed = 10f;

		[Space( 20 )]
		[Tooltip( "How strong the stick force when going down slopes should be." )]
		[SerializeField] private float slopeStickPower = 0.1f;

		[Tooltip( "How fast the acceleration when sliding down slopes should be. Sliding down slopes only happens when the slope's angle is above CharacterController.slopeLimit." )]
		[SerializeField] private float slopeSlidePower = 5f;

		[Tooltip( "How fast the slope slide velocity should decay after the character has safe footing again." )]
		[SerializeField] private float slopeSlideDecaySpeed = 5f;

		[Tooltip( "How far to scan for slopes under the character's feet." )]
		[SerializeField] private float slopeScanDistance = 1f;

		[Space( 20 )]
		[Tooltip( "Amount of raycasts to cast in a circle around the entity to detect edges, higher is better for edge detail, but worse for performance." )]
		[SerializeField] private int edgeScanRaycasts = 16;

		[Tooltip( "How far the edge raycast get cast from the entity." )]
		[SerializeField] private float edgeScanRadius = 0.6f;

		[Tooltip( "How far to scan for edges under the character's feet." )]
		[SerializeField] private float edgeScanDistance = 1.5f;

		[Tooltip( "Like CharacterController.slopeLimit but for edge detection" )]
		[SerializeField] private float edgeScanSlopeLimit = 75f;

		[Space( 20 )]
		[Tooltip( "How far the ground raycast should cast." )]
		[SerializeField] private float movingPlatformGroundRaycastDistance = 2f;

		[Space( 20 )]
		[SerializeField] private float pushDecaySpeed = 10;

		[Header( "Combat" )]
		[Tooltip( "Knockback multiplier" )]
		[SerializeField] private float knockbackStrengthMultiplier = 1;

		[Tooltip( "How fast you return to stationary after taking knockback" )]
		[SerializeField] private float knockbackDecaySpeed = 20;

		[Tooltip( "How long the stun timer can be" )]
		[SerializeField] private float maxStunTime = 3f;

		[Header( "Visuals" )]
		[Tooltip( "Add the GameObject which holds all of the visuals here." )]
		[SerializeField] private GameObject visualsParent;

		[Tooltip( "The lerp speed the visuals rotate at when the entity moves or is told to look somewhere." )]
		[SerializeField] private float visualsRotationSpeed = 15f;

		[Tooltip( "Locks the automatic visuals rotation when input is detected." )]
		[SerializeField] private bool automaticLookRotationLocked;

        [Tooltip("the dash to use, leave empty if dont want a dash")]
        [SerializeField] private Dash dash;

		private readonly Dictionary<string, float> moveSpeedDebuffs = new( );
		private Animator animator;
		private Attacker attacker;

		// Movement
		private CharacterController characterController;

        private bool isDashing = false;

		private Vector3 dashVelocity;

		private Transform ground;

		private Vector3 knockbackVelocity;
		private Transform lastGround;
		private Vector3 lastGroundPosition = Vector3.zero;

		private Vector3 moveVelocity;

		private Vector3 pushVelocity;

		private Vector3 slopeSlideVelocity;

		private EntityState state = EntityState.None;

		private float stunTimer;

		private Vector3 velocity;

		public bool Frozen
		{
			get => frozen;

			set
			{
				frozen = value;

				characterController.enabled = !frozen;
			}
		}

		public float MoveSpeed
		{
			get => moveSpeed;
		}

		public float TargetLookRotation { get; set; }

		private void Awake( )
		{
			characterController = GetComponent<CharacterController>( );
			animator = GetComponentInChildren<Animator>( );

			TryGetComponent( out attacker );
		}

		private void Start( ) { RefreshVisualsRotation( false ); }

		private void Update( )
		{
			if ( !frozen )
			{
				Debug.DrawLine( transform.position, transform.position + GetForwardVector( ), Color.red );

				// Movement
				// NOTE: Reorder movement processing order here!
				ProcessGravity( );
				if ( stunTimer <= 0 ) ProcessMove( );
				ProcessSlope( );
				ProcessKnockback( );
                ProcessDash();
				ProcessPush( );

				ApplyMovement( );

				ProcessMovingPlatform( );
				ProcessEdge( );

				// Visuals
				UpdateVisualsRotation( );
				if ( animator ) UpdateAnimation( );

				RefreshVisualsRotation( );
			}

			if ( stunTimer != 0 ) stunTimer = Mathf.Clamp( stunTimer - Time.deltaTime, 0, maxStunTime );
		}

		private void OnDrawGizmosSelected( )
		{
			Vector3 centerRaycastEndPoint = transform.position + -transform.up * ( GetComponent<CharacterController>( ).height / 2f + edgeScanDistance );
			Gizmos.color = Color.red;
			Gizmos.DrawLine( transform.position, centerRaycastEndPoint );
		}

		private enum EntityState
		{
			None,
			Idle,
			Sprinting,
			Attacking,
			Dashing
		}

		#region Movement

		private void ProcessGravity( )
		{
			if ( characterController.isGrounded )
				velocity.y = -0.5f;
			else
				velocity += gravityScale * Time.deltaTime * Physics.gravity;
		}

		// Add move/walk/run to current velocity
		private void ProcessMove( )
		{
			// Lerp move input vector to create smooth acceleration and deceleration
            if (isDashing)
            {
                velocity = Vector3.zero;
                return;
            }

            moveVelocity = Vector3.Lerp( moveVelocity, inputMoveVector * GetMoveSpeed( ), Time.deltaTime * moveInputVectorLerpSpeed );
			velocity = new( moveVelocity.x, velocity.y, moveVelocity.z );
		}

		private void ProcessSlope( )
		{
			// Raycast for slope
			Physics.Raycast(
				transform.position,
				-transform.up,
				out RaycastHit raycastInfo,
				characterController.height / 2f + slopeScanDistance,
				groundLayers
			);

			if ( raycastInfo.normal != Vector3.zero )
			{
				// Get the angle of the slope the entity is standing on
				float slopeAngle = Vector3.Angle( transform.up, raycastInfo.normal );

				// Apply stick force
				velocity.y -= slopeAngle * slopeStickPower;

				if ( slopeAngle > characterController.slopeLimit )
				{
					// Entity is standing on a slope that is too steep, add slide force
					float slideStrength = slopeAngle / 90f * slopeSlidePower;
					slopeSlideVelocity += slideStrength * Time.deltaTime * new Vector3( raycastInfo.normal.x, 0f, raycastInfo.normal.z ).normalized;
				}
				else
				{
					// Entity is not standing on a slope that is too steep, interpolate slide force to 0 to create a decay effect
					slopeSlideVelocity = Vector3.Lerp( slopeSlideVelocity, Vector3.zero, Time.deltaTime * slopeSlideDecaySpeed );
				}
			}

			velocity += slopeSlideVelocity;
		}

		private void ProcessKnockback( )
		{
			velocity += knockbackVelocity;
			knockbackVelocity = Vector3.Lerp( knockbackVelocity, Vector3.zero, knockbackDecaySpeed * Time.deltaTime );
		}

		private void ProcessDash( )
        {
            dashVelocity = dash?.GetVelocity() ?? Vector3.zero;
			if (isDashing) velocity += dashVelocity;
		}

		private void ProcessPush( )
		{
			pushVelocity = Vector3.Lerp( pushVelocity, Vector3.zero, pushDecaySpeed * Time.deltaTime );
			velocity += pushVelocity;
		}

		private void ProcessEdge( )
		{
			Dictionary<Vector3, float> edgeAdjustments = new( );

			Vector3 centerRaycastEndPoint = transform.position + -transform.up * ( characterController.height / 2f + edgeScanDistance );
			for ( int raycastIndex = 0; raycastIndex < edgeScanRaycasts; raycastIndex++ )
			{
				float positionDegrees = raycastIndex * 360f / edgeScanRaycasts;
				Vector3 raycastLocalPosition = new( Mathf.Cos( positionDegrees * Mathf.Deg2Rad ), 0f, Mathf.Sin( positionDegrees * Mathf.Deg2Rad ) );
				Vector3 raycastGlobalPosition = transform.position + raycastLocalPosition * edgeScanRadius;

				Physics.Raycast(
					raycastGlobalPosition,
					-transform.up,
					out RaycastHit downRaycastInfo,
					characterController.height / 2f + edgeScanDistance,
					groundLayers
				);

				if ( downRaycastInfo.collider ) continue;

				Vector3 downRaycastEndPoint = raycastGlobalPosition + -transform.up * ( characterController.height / 2f + edgeScanDistance );
				Vector3 centerRaycastDirection = ( centerRaycastEndPoint - downRaycastEndPoint ).normalized;
				Physics.Raycast(
					downRaycastEndPoint,
					centerRaycastDirection,
					out RaycastHit centerRaycastInfo,
					edgeScanRadius * 2f,
					groundLayers
				);

				if ( !centerRaycastInfo.collider ) continue;

				float slopeAngle = Vector3.Angle( transform.up, centerRaycastInfo.normal );
				if ( slopeAngle <= edgeScanSlopeLimit ) continue;

				Debug.DrawRay(
					raycastGlobalPosition,
					-transform.up * ( characterController.height / 2f + edgeScanDistance ),
					Color.red,
					0.01f,
					true
				);

				Debug.DrawRay(
					downRaycastEndPoint,
					centerRaycastDirection,
					Color.red,
					0.01f,
					true
				);

				Debug.DrawRay(
					centerRaycastInfo.point,
					centerRaycastInfo.normal,
					Color.blue,
					0.01f,
					true
				);

				if ( edgeAdjustments.Keys.Contains( centerRaycastInfo.normal ) )
				{
					if ( edgeAdjustments[ centerRaycastInfo.normal ] < centerRaycastInfo.distance ) edgeAdjustments[ centerRaycastInfo.normal ] = centerRaycastInfo.distance;
				}
				else
					edgeAdjustments[ centerRaycastInfo.normal ] = centerRaycastInfo.distance;
			}

			foreach ( KeyValuePair<Vector3, float> edgeAdjustment in edgeAdjustments )
			{
				Vector3 adjustment = -edgeAdjustment.Key * Mathf.Max( 0, edgeAdjustment.Value );
				characterController.Move( adjustment );
				Debug.DrawRay(
					transform.position,
					adjustment,
					Color.red,
					0.01f,
					true
				);
			}
		}

		private void ProcessMovingPlatform( )
		{
			Physics.Raycast(
				transform.position,
				Vector3.down,
				out RaycastHit hit,
				movingPlatformGroundRaycastDistance,
				groundLayers
			);

			if ( hit.collider )
			{
				ground = hit.collider.transform;
				if ( ground == lastGround )
				{
					if ( lastGroundPosition != Vector3.zero )
					{
						Vector3 movement = ground.position - lastGroundPosition;
						characterController.Move( movement );

						Debug.DrawLine( transform.position, transform.position + movement * 20, Color.green );
					}
				}
				else
					lastGroundPosition = Vector3.zero;

				lastGroundPosition = ground.position;
			}
			else
			{
				ground = null;
				lastGroundPosition = Vector3.zero;
			}

			lastGround = ground;
		}

		public void Dash( )
		{
			if ( !dash || isDashing || !dash.CanDash()) return;

            StartCoroutine(DashRoutine());
            
			if ( !attacker ) return;

			attacker.EndAttack( true, true );
			attacker.EndAttackCharge( true );
		}

        private IEnumerator DashRoutine()
        {
            isDashing = true;
            
            Vector3 dashDirection = inputMoveVector == Vector3.zero ? GetForwardVector() : inputMoveVector;
            yield return StartCoroutine(dash.PerformDash(dashDirection, this));

            isDashing = false;
        }

		public float GetMoveSpeed( )
		{
			float totalMoveSpeed = moveSpeed;

			foreach ( KeyValuePair<string, float> moveSpeedDebuff in moveSpeedDebuffs ) { totalMoveSpeed -= moveSpeedDebuff.Value; }

			return Mathf.Max( totalMoveSpeed, 0 );
		}

		// Add velocity to CharacterController
		private void ApplyMovement( bool isFixedUpdate = false )
		{
			// Set deltaTime to 1 if method is called by FixedUpdate() message, if not, set to real delta time.
			float deltaTime = isFixedUpdate ? 1 : Time.deltaTime;

			characterController.Move( velocity * deltaTime );
		}

		// Teleport the entity to a new position & y-rotation, if you want to set the entity's transform manually, use EntityCharacterController.frozen instead.
		public void Teleport( Vector3 teleportPosition )
		{
			Frozen = true;

			transform.position = teleportPosition;

			Frozen = false;
		}

        public void Teleport( Vector3 teleportPosition, float teleportYRotation )
		{
			Frozen = true;

			transform.position = teleportPosition;

            TargetLookRotation = teleportYRotation;

			Frozen = false;
		}

        public void Teleport( Vector3 teleportPosition, Vector3 teleportLookDirection )
		{
			Frozen = true;

			transform.position = teleportPosition;
            TargetLookRotation = Quaternion.FromToRotation( Vector3.forward, teleportLookDirection ).eulerAngles.y;

			Frozen = false;
		}

		public void AddMoveSpeedDebuff( string debuffId, float debuff ) { moveSpeedDebuffs[ debuffId ] = debuff; }

		public void RemoveMoveSpeedDebuff( string debuffId ) { moveSpeedDebuffs.Remove( debuffId ); }

		/// <summary>NOTE: Pushes the character according to its current rotation, this means that the direction has to be in local space!</summary>
		/// <param name="direction"></param>
		/// <param name="strength"></param>
		public void PushInDirection( Vector3 direction, float strength )
		{
			Vector3 localPushVelocity = direction.normalized * strength;
			pushVelocity += Quaternion.Euler( 0f, TargetLookRotation, 0f ) * localPushVelocity;
		}

		#endregion

		#region Combat

		public void ApplyKnockback( Vector3 direction, float strength ) { knockbackVelocity = direction * ( strength * knockbackStrengthMultiplier ); }

		public void Stun( float stunDuration ) { stunTimer += stunDuration; }

		#endregion

		#region Visuals

		private void UpdateVisualsRotation( )
		{
			if ( inputMoveVector.magnitude > 0 && !automaticLookRotationLocked ) TargetLookRotation = Mathf.Lerp( TargetLookRotation, Vector3.SignedAngle( transform.forward, inputMoveVector, transform.up ), GetMoveSpeed( ) / moveSpeed );
		}

		private void RefreshVisualsRotation( bool useLerp = true )
		{
			var targetRotation = Quaternion.Euler( 0f, TargetLookRotation, 0f );

			visualsParent.transform.rotation = useLerp ? Quaternion.Lerp( visualsParent.transform.rotation, targetRotation, Time.deltaTime * visualsRotationSpeed ) : targetRotation;
		}

		private void UpdateAnimation( )
		{
			float moveSpeedGoalPercentage = 0;
			if ( moveVelocity.magnitude > 0 ) moveSpeedGoalPercentage = moveVelocity.magnitude / moveSpeed;
			animator.SetFloat( movementSpeed, moveSpeedGoalPercentage );

			if ( attacker && attacker.State != Attacker.CombatState.Idle && attacker.State != Attacker.CombatState.OnCooldown )
			{
				state = EntityState.Attacking;
				return;
			}

			if ( dashVelocity.magnitude > 0.1f )
			{
				if ( state == EntityState.Dashing ) return;

				state = EntityState.Dashing;
				animator.CrossFadeInFixedTime( DASH_ANIMATION_NAME, 0.1f, 0 );
                //animator.CrossFadeInFixedTime( SWORD_ANIMATION_NAME_PREFIX + DASH_ANIMATION_NAME, 0.1f, 1 );

				return;
			}

			if ( new Vector3( inputMoveVector.x, 0, inputMoveVector.z ).magnitude > 0f )
			{
				if ( state == EntityState.Sprinting ) return;

				state = EntityState.Sprinting;
				animator.CrossFadeInFixedTime( SPRINT_ANIMATION_NAME, 0.1f, 0 );
                //animator.CrossFadeInFixedTime( SWORD_ANIMATION_NAME_PREFIX + SPRINT_ANIMATION_NAME, 0.1f, 1 );
				return;
			}

			if ( state != EntityState.Idle )
			{
				state = EntityState.Idle;
				animator.CrossFadeInFixedTime( IDLE_ANIMATION_NAME, 0.1f, 0 );
				//animator.CrossFadeInFixedTime( SWORD_ANIMATION_NAME_PREFIX + IDLE_ANIMATION_NAME, 0.1f, 1 );
			}
		}

		public void LookTowards( Vector3 target, bool useLerp = true )
		{
            Vector3 dir = target - transform.position;
            TargetLookRotation = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            RefreshVisualsRotation(useLerp);
		}

		public Vector3 GetForwardVector( ) => new( -Mathf.Cos( TargetLookRotation * Mathf.Deg2Rad + Mathf.PI / 2 ), 0, Mathf.Sin( TargetLookRotation * Mathf.Deg2Rad + Mathf.PI / 2 ) );

		public void SetAutomaticLookRotationLock( bool isLocked ) { automaticLookRotationLocked = isLocked; }

        public GameObject GetVisualsParent() => visualsParent;

        #endregion
    }
}
