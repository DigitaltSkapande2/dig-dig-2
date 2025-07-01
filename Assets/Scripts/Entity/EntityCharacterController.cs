using UnityEngine;
using UnityEngine.InputSystem;
using DigDig2.Debug;
using DigDig2;

namespace DigDig2
{
	[Debug(DebugMenuToggleable.non_toggleable)]
	public class EntityCharacterController : MonoBehaviour
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
				gameObject.GetComponent<PlayerAttack>().SetFrozen(frozen);
			}
		}
		private bool frozen = false;

		[Header("Movement")]

		[Tooltip("To change the actual gravity value go to Project Settings > Physics > Settings > Gravity, tough this effects all physics.")]
		[SerializeField] private float gravityScale = 1f;

		[Space(20)]

		[SerializeField] private LayerMask groundLayers;

		[Space(20)]

		[Tooltip("The max speed the entity can move.")]
		[DebugSerialized] private float moveSpeed = 7.5f;

		[Tooltip("The direction the entity is currently moving.")]
		[SerializeField] public Vector3 inputMoveVector = Vector3.zero;

		[Tooltip("The move acceleration and decelleration speed, higher is faster.")]
		[SerializeField] private float moveInputVectorLerpSpeed = 10f;

		[Space(20)]

		[Tooltip("How strong the stick force when going down slopes should be.")]
		[SerializeField] private float slopeStickPower = 0.001f;

		[Tooltip("How fast the acceleration when sliding down slopes should be. Sliding down slopes only happens when the slope's angle is above CharacterController.slopeLimit.")]
		[SerializeField] private float slopeSlidePower = 5f;

		[Tooltip("How fast the slope slide velocity should decay after the character has safe footing again.")]
		[SerializeField] private float slopeSlideDecaySpeed = 5f;

		[Tooltip("How far to scan for slopes under the character's feet.")]
		[SerializeField] private float slopeScanDistance = 0.5f;

		[Space(20)]

		[Tooltip("Amount of raycasts to cast in a circle around the entity to detect edges, higher is better for edge detail, but worse for performance.")]
		[SerializeField] private int edgeScanRaycasts = 16;

		[Tooltip("How far the edge raycast get cast from the entity.")]
		[SerializeField] private float edgeScanRadius = 0.6f;

		[Tooltip("How far to scan for edges under the character's feet.")]
		[SerializeField] private float edgeScanDistance = 1.5f;

		[Header("Visuals")]

		[Tooltip("Add the GameObject which holds all of the visuals here.")]
		[SerializeField] private GameObject visualsParent;

		[Tooltip("The lerp speed the visuals rotate at when the entity moves.")]
		[SerializeField] private float visualsMovementRotationSpeed;

		// Movement
		private CharacterController characterController;

		private Vector3 velocity;

		private Vector3 moveVector;

		private Vector3 slopeSlideVelocity;



		private void Awake()
		{
			characterController = GetComponent<CharacterController>();
		}

		private void Start()
		{
			//Application.targetFrameRate = 15;

			DebugNotesManager.Instance.RegisterPlayerCharacterController(this);
		}

		private void Update()
		{
			// Movement
			// NOTE: Reorder movement processing order here!
			ProcessGravity();
			ProcessMove();
			ProcessSlope();
			ProcessEdge();

			if (!frozen) ApplyMovement();

			// Visuals
			UpdateVisualsRotation();
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
			// Lerp move input vector to create smooth acceleration and decelleration
			moveVector = Vector3.Lerp(moveVector, inputMoveVector * moveSpeed, Time.deltaTime * moveInputVectorLerpSpeed);

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

		private void ProcessEdge()
		{
			Vector3 totalEdgeNudge = Vector3.zero;
			int numberOfEdgesDetected = 0;
			for (int raycastIndex = 0; raycastIndex < edgeScanRaycasts; raycastIndex++)
			{
				float positionDegrees = raycastIndex * 360 / edgeScanRaycasts;
				Vector3 raycastPosition = new(Mathf.Cos(positionDegrees * Mathf.Deg2Rad), 0f, Mathf.Sin(positionDegrees * Mathf.Deg2Rad));

				Physics.Raycast(transform.position + raycastPosition * edgeScanRadius, -transform.up, out RaycastHit raycastInfo, characterController.height / 2f + edgeScanDistance, groundLayers);
				if (!raycastInfo.collider)
				{
					UnityEngine.Debug.DrawRay(transform.position + raycastPosition * edgeScanRadius, -transform.up, Color.red, 0.01f, true);

					totalEdgeNudge += raycastPosition * moveVector.magnitude;
					numberOfEdgesDetected++;
				}
			}

			if (numberOfEdgesDetected > 0) velocity -= totalEdgeNudge / numberOfEdgesDetected;
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

		#endregion

		#region Visuals

		private void UpdateVisualsRotation()
		{
			if (inputMoveVector.magnitude > 0 && !frozen)
			{
				Quaternion targetRotation = Quaternion.Euler(0f, Vector3.SignedAngle(transform.forward, inputMoveVector, transform.up), 0f);
				visualsParent.transform.rotation = Quaternion.Lerp(visualsParent.transform.rotation, targetRotation, Time.deltaTime * 10f);
			}
		}

		#endregion
	}
}