using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DebugNotesManager : Singleton<DebugNotesManager>, GameInputSystem.IDebugNotesActions
{
	[Header("Note Storage")]

	[Tooltip("DebugNotesStorage scriptable object used to store note data.")]
	[SerializeField] private DebugNotesStorage debugNotesStorage;

	[Header("Interaction")]

	[Tooltip("Distance from a note the player has to be for it to become focused.")]
	[SerializeField] private float maxFocusDistance = 5f;

	[Tooltip("The offset from the player's position where the note gets placed.")]
	[SerializeField] private Vector3 notePlacePositionOffset = new(0f, 1f, 0f);

	[Header("Note Management Interface")]

	[Tooltip("The note management interface's title.")]
	[SerializeField] private TMP_Text interfaceTitle;

	[Tooltip("The note management interface's title input field.")]
	[SerializeField] private TMP_InputField interfaceTitleInputField;

	[Tooltip("The note management interface's note input field.")]
	[SerializeField] private TMP_InputField interfaceNoteInputField;

	[Tooltip("The note management interface's author input field.")]
	[SerializeField] private TMP_InputField interfaceAuthorInputField;

	[Tooltip("The note management interface's archive button")]
	[SerializeField] private Button interfaceArchiveButton;

	[Tooltip("The note management interface's canvas.")]
	[SerializeField] private GameObject interfaceCanvas;

	[Header("Prefabs")]

	[Tooltip("The in-game note billboard prefab.")]
	[SerializeField] private DebugNoteBillboard debugNoteBillboardPrefab;

	private GameInputSystem.DebugNotesActions debugNotesActions;

	private PlayerCharacterController playerCharacterController;

	readonly private Dictionary<int, DebugNoteBillboard> debugNoteBillboards = new();
	readonly private Dictionary<int, DebugNoteBillboard> archivedDebugNoteBillboards = new();

	private int focusedNoteIndex;
	private int editingNoteIndex;
	private NoteManagementMode noteManagementMode = NoteManagementMode.none;

	public bool ShowDebugNotes
	{
		get
		{
			return _showDebugNotes;
		}

		set
		{
			_showDebugNotes = value;
			RefreshNoteBillboardVisibility();
		}
	}
	private bool _showDebugNotes = true;

	public bool ShowArchivedDebugNotes
	{
		get
		{
			return _showArchivedDebugNotes;
		}

		set
		{
			_showArchivedDebugNotes = value;
			RefreshNoteBillboardVisibility();
		}
	}
	private bool _showArchivedDebugNotes = false;

	enum NoteManagementMode
	{
		none,
		place,
		edit
	}




	private void Start()
	{
		PlaceStoredDebugNotes();

		HideNoteManagementInterface();

		EnableInput();
	}

	private void Update()
	{
		// Maybe not too performace friendly to check all notes in a for loop every frame, but uhhhhhhhhhhhhh i don't know any easier way :)
		FocusNearestNote();
	}

	#region Notes Backend

	// Get the nearest note and focus it, if a note is already focused then unfocus the old one
	private void FocusNearestNote()
	{
		if (playerCharacterController == null)
		{
			if (focusedNoteIndex != -1)
			{
				DebugNoteBillboard focusedNoteBillboard = GetNoteBillboardFromNoteIndex(focusedNoteIndex);

				focusedNoteBillboard.SetFocused(false);
				focusedNoteIndex = -1;
			}
			return;
		}

		DebugNoteBillboard nearestNoteBillboard = null;
		int nearestNote = -1;
		float nearestNoteDistance = 0;
		foreach (int noteIndex in debugNoteBillboards.Keys)
		{
			DebugNoteBillboard noteBillboard = GetNoteBillboardFromNoteIndex(noteIndex);
			if (!noteBillboard.gameObject.activeSelf) continue;

			float noteDistance = Vector3.Distance(playerCharacterController.transform.position, noteBillboard.transform.position);
			if (noteDistance <= maxFocusDistance)
			{
				if (nearestNote == -1)
				{
					nearestNoteBillboard = noteBillboard;
					nearestNote = noteIndex;
					nearestNoteDistance = noteDistance;

					continue;
				}

				if (noteDistance < nearestNoteDistance)
				{
					nearestNoteBillboard = noteBillboard;
					nearestNote = noteIndex;
				}
			}
		}

		if (nearestNote != -1)
		{
			if (focusedNoteIndex != -1)
			{
				DebugNoteBillboard focusedNoteBillboard = GetNoteBillboardFromNoteIndex(focusedNoteIndex);

				focusedNoteBillboard.SetFocused(false);
			}

			focusedNoteIndex = nearestNote;
			nearestNoteBillboard.SetFocused(true);
		}
		else
		{
			if (focusedNoteIndex != -1)
			{
				DebugNoteBillboard focusedNoteBillboard = GetNoteBillboardFromNoteIndex(focusedNoteIndex);

				focusedNoteBillboard.SetFocused(false);
				focusedNoteIndex = -1;
			}
		}
	}

	private void RefreshNoteBillboardVisibility()
	{
		foreach (int noteIndex in debugNoteBillboards.Keys)
		{
			DebugNoteBillboard noteBillboard = GetNoteBillboardFromNoteIndex(noteIndex);
			if (!_showDebugNotes)
			{
				noteBillboard.gameObject.SetActive(false);
				continue;
			}

			DebugNoteData noteData = GetNoteDataFromNoteIndex(noteIndex);
			if (!_showArchivedDebugNotes && noteData.archived)
			{
				noteBillboard.gameObject.SetActive(false);
				continue;
			}

			noteBillboard.gameObject.SetActive(true);
		}
	}

	private DebugNoteBillboard GetNoteBillboardFromNoteIndex(int noteIndex)
	{
		if (debugNoteBillboards.ContainsKey(noteIndex))
		{
			return debugNoteBillboards[noteIndex];
		}

		return null;
	}
	private DebugNoteData GetNoteDataFromNoteIndex(int noteIndex)
	{
		if (debugNotesStorage.notes.Count > noteIndex)
		{
			return debugNotesStorage.notes[noteIndex];
		}

		return null;
	}

	// Place an existing note in the world, does not save a new note to the storage
	private void PlaceStoredDebugNote(int noteIndex)
	{
		DebugNoteData noteData = GetNoteDataFromNoteIndex(noteIndex);

		DebugNoteBillboard newNoteBillboard = Instantiate(debugNoteBillboardPrefab, noteData.position, Quaternion.identity, transform);
		newNoteBillboard.ApplyNoteData(noteData);

		if (noteData.archived)
		{
			newNoteBillboard.gameObject.SetActive(_showDebugNotes && _showArchivedDebugNotes);
		}
		else
		{
			newNoteBillboard.gameObject.SetActive(_showDebugNotes);
		}

		debugNoteBillboards.Add(noteIndex, newNoteBillboard);
	}

	// Go through the stored notes and place them in the world
	private void PlaceStoredDebugNotes()
	{
		for (int noteIndex = 0; noteIndex < debugNotesStorage.notes.Count; noteIndex++)
		{
			PlaceStoredDebugNote(noteIndex);
		}
	}

	// Create a new note in the note storage and place it in the world
	public void CreateNewNote(string title, string note, string author, Vector3 position)
	{
		DebugNoteData newNoteData = new()
		{
			title = title,
			note = note,
			author = author,
			position = position
		};

		debugNotesStorage.notes.Add(newNoteData);
		int newNoteIndex = debugNotesStorage.notes.Count - 1;

		PlaceStoredDebugNote(newNoteIndex);
	}

	public void EditNote(int noteIndex, string title, string note, string author)
	{
		DebugNoteData noteData = GetNoteDataFromNoteIndex(noteIndex);
		DebugNoteBillboard noteBillboard = GetNoteBillboardFromNoteIndex(noteIndex);

		noteData.title = title;
		noteData.note = note;
		noteData.author = author;

		noteBillboard.ApplyNoteData(noteData);
	}

	public void ArchiveNote(int noteIndex)
	{
		DebugNoteData noteData = GetNoteDataFromNoteIndex(noteIndex);
		if (noteData.archived)
		{
			Debug.LogError($"Debug note with index \"{noteIndex}\" was requested to be archived but is already archived!");
			return;
		}


		noteData.archived = true;

		DebugNoteBillboard noteBillboard = GetNoteBillboardFromNoteIndex(noteIndex);
		noteBillboard.ApplyNoteData(noteData);
		noteBillboard.gameObject.SetActive(_showDebugNotes && _showArchivedDebugNotes);
	}
	
	public void UnarchiveNote(int archivedNoteIndex)
	{
		DebugNoteData noteData = GetNoteDataFromNoteIndex(archivedNoteIndex);
		if (!noteData.archived)
		{
			Debug.LogError($"Archived debug note with index \"{archivedNoteIndex}\" was requested to be unarchived but was not archived!");
			return;
		}

		noteData.archived = false;

		DebugNoteBillboard noteBillboard = GetNoteBillboardFromNoteIndex(archivedNoteIndex);
		noteBillboard.ApplyNoteData(noteData);
		noteBillboard.gameObject.SetActive(_showDebugNotes);
	}

	// Register the current player character controller to be used for getting the position and freezing the player
	public void RegisterPlayerCharacterController(PlayerCharacterController newPlayerCharacterController)
	{
		playerCharacterController = newPlayerCharacterController;
	}

	#endregion

	#region Notes Frontend

	public void StartNotePlacement()
	{
		if (noteManagementMode != NoteManagementMode.none) return;

		ShowNoteManagementInterface(NoteManagementMode.place);
	}

	public void StartNoteEditing()
	{
		if (noteManagementMode != NoteManagementMode.none) return;
		if (focusedNoteIndex == -1) return;

		editingNoteIndex = focusedNoteIndex;
		DebugNoteData editingNoteData = GetNoteDataFromNoteIndex(editingNoteIndex);

		interfaceArchiveButton.interactable = true;

		interfaceTitleInputField.text = editingNoteData.title;
		interfaceNoteInputField.text = editingNoteData.note;
		interfaceAuthorInputField.text = editingNoteData.author;

		ShowNoteManagementInterface(NoteManagementMode.edit);
	}

	private void ShowNoteManagementInterface(NoteManagementMode managementMode)
	{
		noteManagementMode = managementMode;
		switch (managementMode)
		{
			case NoteManagementMode.place:
				interfaceTitle.text = "Place Note";
				break;
			case NoteManagementMode.edit:
				interfaceTitle.text = "Edit Note";
				break;
		}

		interfaceCanvas.SetActive(true);

		playerCharacterController.Frozen = true;
	}
	private void HideNoteManagementInterface()
	{
		interfaceCanvas.SetActive(false);

		playerCharacterController.Frozen = false;

		interfaceArchiveButton.interactable = false;

		interfaceTitleInputField.text = "";
		interfaceNoteInputField.text = "";
		interfaceAuthorInputField.text = "";

		noteManagementMode = NoteManagementMode.none;
	}

	// Player pressed the confirm button on the note management interface
	public void OnNoteConfirmPressed()
	{
		if (playerCharacterController == null) return;
		if (interfaceTitleInputField.text.Length <= 0) return;

		switch (noteManagementMode)
		{
			case NoteManagementMode.place:
				CreateNewNote(
					interfaceTitleInputField.text,
					interfaceNoteInputField.text,
					interfaceAuthorInputField.text,
					playerCharacterController.transform.position + notePlacePositionOffset
				);
				break;
			case NoteManagementMode.edit:
				EditNote(
					editingNoteIndex,
					interfaceTitleInputField.text,
					interfaceNoteInputField.text,
					interfaceAuthorInputField.text
				);
				break;
		}

		HideNoteManagementInterface();
	}

	public void OnNoteArchivePressed()
	{
		if (editingNoteIndex == -1) return;
		DebugNoteData editingNoteData = GetNoteDataFromNoteIndex(editingNoteIndex);

		if (editingNoteData.archived)
		{
			UnarchiveNote(editingNoteIndex);
		}
		else
		{
			ArchiveNote(editingNoteIndex);
		}

		HideNoteManagementInterface();
	}

	public void OnNoteCancelPressed()
	{
		HideNoteManagementInterface();
	}

	#endregion

	#region Input Setup

	private void EnableInput()
	{
		debugNotesActions = GameInputManager.Instance.gameInputSystem.DebugNotes;

		debugNotesActions.SetCallbacks(this);
		debugNotesActions.Enable();
	}

	private void DisableInput()
	{
		debugNotesActions.Disable();
	}

	#endregion

	#region Input

	public void OnPlaceNote(InputAction.CallbackContext context)
	{
		StartNotePlacement();
	}

	public void OnEditNote(InputAction.CallbackContext context)
	{
		StartNoteEditing();
	}

	#endregion
}
