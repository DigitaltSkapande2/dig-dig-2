using UnityEngine;
using UnityEngine.InputSystem;
using DigDig2.Debug;
using DigDig2;

[Debug(DebugMenuToggleable.non_toggleable)]
public class PlayerCharacterController : MonoBehaviour, GameInputSystem.IPlayerActions
{
	// These get set public bools are supposed to be serialized but unity or C# doesn't allow that so fix this please!
	[Tooltip("Freezes the player (stops running ´CharacterController.Move()´) and disables the CharacterController component. Allows you to move the player by setting their transform. Use TempPlayerCharacterController.Teleport() for teleporting instead.")]
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

	[Tooltip("Resets the input values and stops accepting new input, use for menus or when in dialog.")]
	[SerializeField]
	private bool AcceptInput
	{
		get
		{
			return acceptInput;
		}

		set
		{
			acceptInput = value;

			if (!acceptInput)
			{
				ResetInputValues();
			}
		}
	}
	private bool acceptInput = true;

	[Header("Movement")]

	[Tooltip("To change the actual gravity value go to Project Settings > Physics > Settings > Gravity, tough this effects all physics.")]
	[SerializeField] private float gravityScale = 1f;

	[Space(20)]

	[SerializeField] private LayerMask groundLayers;

	[Space(20)]

	[Tooltip("The max speed the player can move.")]
	[DebugSerialized] private float moveSpeed = 7.5f;

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

	[Tooltip("Amount of raycasts to cast in a circle around the player to detect edges, higher is better for edge detail, but worse for performance.")]
	[SerializeField] private int edgeScanRaycasts = 16;

	[Tooltip("How far the edge raycast get cast from the player.")]
	[SerializeField] private float edgeScanRadius = 0.6f;

	[Tooltip("How far to scan for edges under the character's feet.")]
	[SerializeField] private float edgeScanDistance = 1.5f;

	[Header("Visuals")]

	[Tooltip("Add the GameObject which holds all of the visuals here.")]
	[SerializeField] private GameObject visualsParent;

	[Tooltip("The lerp speed the visuals rotate at when the player moves.")]
	[SerializeField] private float visualsMovementRotationSpeed;

	// Input
	private GameInputSystem.PlayerActions playerActions;

	// Movement
	private CharacterController characterController;

	private Vector3 velocity;

	private Vector3 moveVector;
	private Vector2 moveInputVector;

	private Vector3 slopeSlideVelocity;

	private Interactor interactor;



	private void Awake()
	{
		characterController = GetComponent<CharacterController>();
		interactor = GetComponentInChildren<Interactor>();
	}

	private void Start()
	{
		//Application.targetFrameRate = 15;

		DebugNotesManager.Instance.RegisterPlayerCharacterController(this);

		EnableInput();
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

	private void OnEnable()
	{
		//EnableInput();
	}
	private void OnDisable()
	{
		DisableInput();
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
		if (Camera.main)
		{
			// Rotate moveInputVector to the player's camera
			Vector3 rotatedMoveInputVector = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f) * new Vector3(moveInputVector.x, 0f, moveInputVector.y);

			// Lerp move input vector to create smooth acceleration and decelleration
			moveVector = Vector3.Lerp(moveVector, rotatedMoveInputVector * moveSpeed, Time.deltaTime * moveInputVectorLerpSpeed);

			velocity = new(moveVector.x, velocity.y, moveVector.z);
		}
	}

	private void ProcessSlope()
	{
		// Raycast for slope
		Physics.Raycast(transform.position, -transform.up, out RaycastHit raycastInfo, characterController.height / 2f + slopeScanDistance, groundLayers);
		if (raycastInfo.normal != null)
		{
			// Get the angle of the slope the player is standing on
			float slopeAngle = Vector3.Angle(transform.up, raycastInfo.normal);

			// Apply stick force
			velocity.y -= slopeAngle * slopeStickPower;

			if (slopeAngle > characterController.slopeLimit)
			{
				// Player is standing on a slope that is too steep, add slide force
				float slideStrength = slopeAngle / 90f * slopeSlidePower;
				slopeSlideVelocity += slideStrength * Time.deltaTime * new Vector3(raycastInfo.normal.x, 0f, raycastInfo.normal.z).normalized;
			}
			else
			{
				// Player is not standing on a slope that is too steep, interpolate slide force to 0 to create a decay effect
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
				Debug.DrawRay(transform.position + raycastPosition * edgeScanRadius, -transform.up, Color.red, 0.01f, true);

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

	// Teleport the player to a new position & y-rotation, if you want to set the player's transform manually, use TempPlayerCharacterController.frozen instead.
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
		if (moveInputVector.magnitude > 0 && !frozen)
		{
			// Rotate moveInputVector to the player's camera
			Vector3 rotatedMoveInputVector = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f) * new Vector3(moveInputVector.x, 0f, moveInputVector.y);

			Quaternion targetRotation = Quaternion.Euler(0f, Vector3.SignedAngle(transform.forward, rotatedMoveInputVector, transform.up), 0f);
			visualsParent.transform.rotation = Quaternion.Lerp(visualsParent.transform.rotation, targetRotation, Time.deltaTime * 10f);
		}
	}

	#endregion

	#region Input Setup

	private void EnableInput()
	{
		playerActions = GameInputManager.Instance.gameInputSystem.Player;

		playerActions.SetCallbacks(this);
		playerActions.Enable();
	}

	private void DisableInput()
	{
		playerActions.Disable();
	}

	#endregion

	#region Input Utilities

	public void ResetInputValues()
	{
		moveInputVector = Vector2.zero;
	}

	#endregion

	#region Input Action Callbacks

	public void OnMove(InputAction.CallbackContext context)
	{
		moveInputVector = context.ReadValue<Vector2>();
	}

	public void OnAttack(InputAction.CallbackContext context)
	{
		GetComponent<PlayerAttack>().OnAttack(context);
	}

	public void OnMouse(InputAction.CallbackContext context)
	{
		GetComponent<PlayerAttack>().OnMouse(context);
	}

	public void OnInteract(InputAction.CallbackContext context)
	{
		if (interactor) interactor.SendInteraction(context.phase);
	}

	public void OnSprint(InputAction.CallbackContext context)
	{

	}

	#endregion
}
