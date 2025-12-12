using UnityEngine;
using DigDig2.Debugging;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using System.Collections;
using System;

namespace DigDig2
{
	// This might be kaboom?
	[Debug(DebugMenuToggleable.non_toggleable), RequireComponent(typeof(CharacterController))]
	public class EntityCharacterController : NetworkBehaviour
	{
		// These get set public bools are supposed to be serialized but unity or C# doesn't allow that so fix this please!
		[Tooltip("Freezes the entity (stops running ´CharacterController.Move()´) and disables the CharacterController component. Allows you to move the entity by setting their transform. Use EntityCharacterController.Teleport() for teleporting instead.")]
		[SerializeField]
		public bool Frozen
		{
			get
			{
				return frozen;
			}

			set
			{
				frozen = value;

				characterController.enabled = !frozen;
				//gameObject.GetComponent<PlayerAttack>().SetFrozen(frozen);
			}
		}
		private bool frozen = false;

		[Header("Movement")]

		[Tooltip("To change the actual gravity value go to Project Settings > Physics > Settings > Gravity, tough this effects all physics.")]
		[SerializeField] private float gravityScale = 1f;

		[Space(20)]

		[SerializeField] private LayerMask groundLayers;

		[Space(20)]

		[Tooltip("The max speed the entity can walk at.")]
		[SerializeField, DebugSerialized] private float moveSpeed = 5f;

		[Tooltip("The mac speed the entity can sprint at.")]
		[SerializeField, DebugSerialized] private float attackMoveSpeed = 0.5f;

		[Tooltip("The direction the entity is currently moving.")]
		[SerializeField] public Vector3 inputMoveVector = Vector3.zero;

		[Tooltip("The move acceleration and decelleration speed, higher is faster.")]
		[SerializeField] private float moveInputVectorLerpSpeed = 10f;

		[Tooltip("If the entity is sprinting or not, moveSpeed is default speed, sprintMoveSpeed is sprint speed.")]
		[SerializeField] public bool isAttacking = false;

		[Space(20)]

		[Tooltip("How strong the stick force when going down slopes should be.")]
		[SerializeField] private float slopeStickPower = 0.1f;

		[Tooltip("How fast the acceleration when sliding down slopes should be. Sliding down slopes only happens when the slope's angle is above CharacterController.slopeLimit.")]
		[SerializeField] private float slopeSlidePower = 5f;

		[Tooltip("How fast the slope slide velocity should decay after the character has safe footing again.")]
		[SerializeField] private float slopeSlideDecaySpeed = 5f;

		[Tooltip("How far to scan for slopes under the character's feet.")]
		[SerializeField] private float slopeScanDistance = 1f;

		[Space(20)]

		[Tooltip("Amount of raycasts to cast in a circle around the entity to detect edges, higher is better for edge detail, but worse for performance.")]
		[SerializeField] private int edgeScanRaycasts = 16;

		[Tooltip("How far the edge raycast get cast from the entity.")]
		[SerializeField] private float edgeScanRadius = 0.6f;

		[Tooltip("How far to scan for edges under the character's feet.")]
		[SerializeField] private float edgeScanDistance = 1.5f;

		[Tooltip("Like CharacterController.slopeLimit but for edge detection")]
		[SerializeField] private float edgeScanSlopeLimit = 75f;

		[Space(20)]

		[Tooltip("How far the ground raycast should cast")]
		[SerializeField] private float movingPlatformGroundRaycastDistance = 2f;

		[Header("Combat")]

		[Tooltip("Knockback multiplier")]
		[SerializeField] private float knockbackMultiplier;

		[Tooltip("How fast you return to stationary after taking knockback")]
		[SerializeField] private float knockbackFallofSpeed;

		[Tooltip("How long the stun timer can be")]
		[SerializeField] private float maxStunTime = 3f;

		[Header("Visuals")]

		[Tooltip("Add the GameObject which holds all of the visuals here.")]
		[SerializeField] private GameObject visualsParent;

		[Tooltip("The lerp speed the visuals rotate at when the entity moves or is told to look somewhere.")]
		[SerializeField] private float visualsRotationSpeed = 15f;

		[Tooltip("Locks the automatic visuals rotation when input is detected.")]
		[SerializeField] private bool automaticLookRotationLocked = false;

		// Movement
		private CharacterController characterController;

		private Vector3 velocity;
		private Animator animator;

		private Vector3 moveVector;

		private float slowDownTimer;

		private Vector3 slopeSlideVelocity;

		private Vector3 knockbackVelocity;

		private float stunTimer = 0;

		private Transform ground;
		private Transform lastGround;
		private Vector3 lastGroundPosition = Vector3.zero;

		[SyncVar] private float targetLookRotation = 0f;

		private enum PlayerState
        {
            Idle,
			Sprinting,
        }

		private PlayerState state;

		private void Awake()
		{
			characterController = GetComponent<CharacterController>();
			animator = GetComponentInChildren<Animator>();
		}

		private void Start()
		{
			if (isClient)
			{
				RefreshVisualsRotation(false);
			}
		}

		private void Update()
		{
			if (!frozen)
			{
				if (authority)
				{
					Debug.DrawLine(transform.position, transform.position + GetForwardVector(), Color.red);

					// Movement
					// NOTE: Reorder movement processing order here!
					ProcessGravity();
					if (stunTimer <= 0) ProcessMove();
					ProcessSlope();
					ProcessKnockback();

					ApplyMovement();

					ProcessMovingPlatform();
					ProcessEdge();

					// Visuals
					UpdateVisualsRotation();
					UpdateAnimation();
				}
				else
                {
                    frozen = true;
                }
			}

			if (isClient)
			{
				RefreshVisualsRotation();
			}

			if (authority)
			{
				if (stunTimer != 0)
				{
					stunTimer = Mathf.Clamp(stunTimer - Time.deltaTime, 0, maxStunTime);
				}
			}
		}

		#region Movement

		private void ProcessGravity()
		{
			if (characterController.isGrounded)
			{
				velocity.y = -0.5f;
			}
			else
			{
				velocity += Physics.gravity * gravityScale * Time.deltaTime;
			}
		}

		// Add move/walk/run to current velocity
		private void ProcessMove()
		{
			slowDownTimer -= Time.deltaTime;
			isAttacking = slowDownTimer > 0;

			float speed = isAttacking ? attackMoveSpeed : moveSpeed;

			// Lerp move input vector to create smooth acceleration and decelleration
			moveVector = Vector3.Lerp(moveVector, inputMoveVector * speed, Time.deltaTime * moveInputVectorLerpSpeed);

			velocity = new(moveVector.x, velocity.y, moveVector.z);
		}

		private void ProcessSlope()
		{
			// Raycast for slope
			Physics.Raycast(transform.position, -transform.up, out RaycastHit raycastInfo, characterController.height / 2f + slopeScanDistance, groundLayers);
			if (raycastInfo.normal != null)
			{
				// Get the angle of the slope the entity is standing on
				float slopeAngle = Vector3.Angle(transform.up, raycastInfo.normal);

				// Apply stick force
				velocity.y -= slopeAngle * slopeStickPower;

				if (slopeAngle > characterController.slopeLimit)
				{
					// Entity is standing on a slope that is too steep, add slide force
					float slideStrength = slopeAngle / 90f * slopeSlidePower;
					slopeSlideVelocity += slideStrength * Time.deltaTime * new Vector3(raycastInfo.normal.x, 0f, raycastInfo.normal.z).normalized;
				}
				else
				{
					// Entity is not standing on a slope that is too steep, interpolate slide force to 0 to create a decay effect
					slopeSlideVelocity = Vector3.Lerp(slopeSlideVelocity, Vector3.zero, Time.deltaTime * slopeSlideDecaySpeed);
				}
			}

			velocity += slopeSlideVelocity;
		}

		private void ProcessKnockback()
		{
			velocity += knockbackVelocity;
			knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackFallofSpeed * Time.deltaTime);
		}

		private void ProcessEdge()
		{
			Dictionary<Vector3, float> edgeAdjustments = new();

			Vector3 centerRaycastEndPoint = transform.position + -transform.up * (characterController.height / 2f + edgeScanDistance);
			for (int raycastIndex = 0; raycastIndex < edgeScanRaycasts; raycastIndex++)
			{
				float positionDegrees = raycastIndex * 360 / edgeScanRaycasts;
				Vector3 raycastLocalPosition = new(Mathf.Cos(positionDegrees * Mathf.Deg2Rad), 0f, Mathf.Sin(positionDegrees * Mathf.Deg2Rad));
				Vector3 raycastGlobalPosition = transform.position + raycastLocalPosition * edgeScanRadius;

				Physics.Raycast(raycastGlobalPosition, -transform.up, out RaycastHit downRaycastInfo, characterController.height / 2f + edgeScanDistance, groundLayers);
				if (!downRaycastInfo.collider)
				{
					Vector3 downRaycastEndPoint = raycastGlobalPosition + -transform.up * (characterController.height / 2f + edgeScanDistance);
					Vector3 centerRaycastDirection = (centerRaycastEndPoint - downRaycastEndPoint).normalized;
					Physics.Raycast(downRaycastEndPoint, centerRaycastDirection, out RaycastHit centerRaycastInfo, edgeScanRadius * 2f, groundLayers);

					if (!centerRaycastInfo.collider) continue;
					float slopeAngle = Vector3.Angle(transform.up, centerRaycastInfo.normal);
					if (slopeAngle <= edgeScanSlopeLimit) continue;

					Debug.DrawRay(raycastGlobalPosition, -transform.up * (characterController.height / 2f + edgeScanDistance), Color.red, 0.01f, true);
					Debug.DrawRay(downRaycastEndPoint, centerRaycastDirection, Color.red, 0.01f, true);
					Debug.DrawRay(centerRaycastInfo.point, centerRaycastInfo.normal, Color.blue, 0.01f, true);

					if (edgeAdjustments.Keys.Contains(centerRaycastInfo.normal))
					{
						if (edgeAdjustments[centerRaycastInfo.normal] < centerRaycastInfo.distance) edgeAdjustments[centerRaycastInfo.normal] = centerRaycastInfo.distance;
					}
					else
					{
						edgeAdjustments[centerRaycastInfo.normal] = centerRaycastInfo.distance;
					}
				}
			}

			foreach (KeyValuePair<Vector3, float> edgeAdjustment in edgeAdjustments)
			{
				Vector3 adjustment = -edgeAdjustment.Key * Mathf.Max(0, edgeAdjustment.Value);
				characterController.Move(adjustment);
				Debug.DrawRay(transform.position, adjustment, Color.red, 0.01f, true);
			}
		}

		private void ProcessMovingPlatform()
		{
			Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, movingPlatformGroundRaycastDistance, groundLayers);
			if (hit.collider)
			{
				ground = hit.collider.transform;
				if (ground == lastGround)
				{
					if (lastGroundPosition != Vector3.zero)
					{
						Vector3 movement = ground.position - lastGroundPosition;
						characterController.Move(movement);

						Debug.DrawLine(transform.position, transform.position + movement * 20, Color.green);
					}
				}
				else
				{
					lastGroundPosition = Vector3.zero;
				}

				lastGroundPosition = ground.position;
			}
			else
			{
				ground = null;
				lastGroundPosition = Vector3.zero;
			}

			lastGround = ground;
		}

		// Add velocity to CharacterController
		private void ApplyMovement(bool isFixedUpdate = false)
		{
			// Set deltaTime to 1 if method is called by FixedUpdate() message, if not, set to real delta time.
			float deltaTime = isFixedUpdate ? 1 : Time.deltaTime;

			characterController.Move(velocity * deltaTime);
		}

		// Teleport the entity to a new position & y-rotation, if you want to set the entity's transform manually, use EntityCharacterController.frozen instead.
		private void Teleport(Vector3 teleportPosition)
		{
			Frozen = true;

			transform.position = teleportPosition;

			Frozen = false;
		}
		private void Teleport(Vector3 teleportPosition, float teleportYRotation)
		{
			Frozen = true;

			transform.position = teleportPosition;

			// Apply teleportYRotation
			Vector3 currentRotation = transform.rotation.eulerAngles;
			currentRotation.y = teleportYRotation;
			transform.rotation = Quaternion.Euler(currentRotation);

			Frozen = false;
		}
		private void Teleport(Vector3 teleportPosition, Vector3 teleportLookDirection)
		{
			Frozen = true;

			transform.position = teleportPosition;

			// Apply teleportLookDirection
			Vector3 currentRotation = transform.rotation.eulerAngles;
			currentRotation.y = Quaternion.FromToRotation(Vector3.forward, teleportLookDirection).eulerAngles.y;
			transform.rotation = Quaternion.Euler(currentRotation);

			Frozen = false;
		}

		public void AttackSlowdown(float time)
        {
            slowDownTimer = time;
        }


		#endregion

		#region Combat

		public void ApplyKnockback(Vector3 knockbackForce)
		{
			knockbackVelocity = knockbackForce * knockbackMultiplier;
		}
		
		public void Stun(float stunDuration)
		{
			stunTimer += stunDuration;
        }

		#endregion

		#region Visuals

		private void UpdateVisualsRotation()
		{
			if (inputMoveVector.magnitude > 0 && !automaticLookRotationLocked)
			{
				targetLookRotation = Vector3.SignedAngle(transform.forward, inputMoveVector, transform.up);
			}
		}

		private void RefreshVisualsRotation(bool useLerp = true)
		{
			Quaternion targetRotation = Quaternion.Euler(0f, targetLookRotation, 0f);

			if (useLerp) visualsParent.transform.rotation = Quaternion.Lerp(visualsParent.transform.rotation, targetRotation, Time.deltaTime * visualsRotationSpeed);
			else visualsParent.transform.rotation = targetRotation;
		}

		private void UpdateAnimation()
        {
            if (new Vector3(inputMoveVector.x, 0, inputMoveVector.z).magnitude > 0f)
            {
				if (state == PlayerState.Sprinting) return;

                animator.CrossFadeInFixedTime("Sprint", 0.1f, 0);
				animator.CrossFadeInFixedTime("SwordSprint", 0.1f, 1);
				state = PlayerState.Sprinting;
				return;
            }

			if (state == PlayerState.Idle) return;

			state = PlayerState.Idle;
			animator.CrossFadeInFixedTime("Idle", 0.1f, 0);
			animator.CrossFadeInFixedTime("SwordIdle", 0.1f, 1);
        }

		public void LookTowards(Vector3 target)
		{
			targetLookRotation = Vector3.SignedAngle(transform.forward, target - transform.position, transform.up);
		}

		public Vector3 GetForwardVector()
        {
			return new Vector3(-Mathf.Cos(targetLookRotation * Mathf.Deg2Rad + Mathf.PI/2), 0, Mathf.Sin(targetLookRotation * Mathf.Deg2Rad + Mathf.PI/2));
        }

		public void SetAutomaticLookRotationLock(bool isLocked)
		{
			automaticLookRotationLocked = isLocked;
		}

		#endregion
	}
}