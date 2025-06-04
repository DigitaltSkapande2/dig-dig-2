using UnityEngine;
using UnityEngine.InputSystem;

public class TempPlayerCharacterController : MonoBehaviour, GameInputSystem.IPlayerActions
{
	[Tooltip("Freezes the player (stops running ´CharacterController.Move()´) and disables the CharacterController component. Allows you to move the player by setting their transform. Use TempPlayerCharacterController.Teleport() for teleporting instead.")]
	[SerializeField] public bool Frozen
	{
		get
		{
			return frozen;
		}

		set
		{
			frozen = value;

			characterController.enabled = !frozen;
		}
	}
	private bool frozen = false;

	[Tooltip("Resets the input values and stops accepting new input, use for menus or when in dialog.")]
	[SerializeField] private bool AcceptInput
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

	[Tooltip("The max speed the player can move.")]
	[SerializeField] private float moveSpeed = 7.5f;

	[Tooltip("The move acceleration and decelleration speed, higher is faster.")]
	[SerializeField] private float moveInputVectorLerpSpeed = 10f;

	// Input
	private GameInputSystem.PlayerActions playerActions;

	// Movement
	private CharacterController characterController;

	private Vector3 velocity;

	private Vector3 moveVector;
	private Vector2 moveInputVector;



	private void Awake()
	{
		characterController = GetComponent<CharacterController>();
	}

	private void Start()
	{
		EnableInput();
	}

	private void Update()
	{
		// Movement
		velocity = characterController.velocity;

		// NOTE: Reorder movement processing order here!
		ProcessGravity();
		ProcessMove();

		if (!frozen) ApplyMovement();
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
		velocity += Physics.gravity * gravityScale * Time.deltaTime;
	}

	// Add move/walk/run to current velocity
	private void ProcessMove()
	{
		if (Camera.main)
		{
			Vector3 rotatedMoveInputVector = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f) * new Vector3(moveInputVector.x, 0f, moveInputVector.y);

			// Lerp move input vector to create smooth acceleration and decelleration
			moveVector = Vector3.Lerp(moveVector, rotatedMoveInputVector * moveSpeed, Time.deltaTime * moveInputVectorLerpSpeed);

			velocity = new(moveVector.x, velocity.y, moveVector.z);
		}
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
		Debug.Log(moveInputVector);
	}

    public void OnAttack(InputAction.CallbackContext context)
    {
        
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        
    }
    
    #endregion
}
