using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugNotesManager : Singleton<DebugNotesManager>, GameInputSystem.IDebugNotesActions
{
	[SerializeField] private DebugNotesStorage debugNotesStorage;
	[SerializeField] private DebugNoteBillboard debugNoteBillboardPrefab;
	[SerializeField] private float maxFocusDistance = 5f;
	[SerializeField] private TMP_InputField newNoteTitleInputField;
	[SerializeField] private TMP_InputField newNoteNoteInputField;
	[SerializeField] private GameObject placeNoteScreen;
	[SerializeField] private Vector3 notePlacePositionOffset = new(0f, 1f, 0f);

	private List<DebugNoteBillboard> debugNoteBillboards = new();
	private DebugNoteBillboard focusedNoteBillboard;
	private PlayerCharacterController playerCharacterController;
	private GameInputSystem.DebugNotesActions debugNotesActions;



	private void Start()
	{
		PlaceStoredDebugNotes();

		EnableInput();
	}

	private void Update()
	{
		FocusNearestNote();
	}

	private void FocusNearestNote()
	{
		if (playerCharacterController == null)
		{
			if (focusedNoteBillboard) { focusedNoteBillboard.SetFocused(false); focusedNoteBillboard = null; }
			return;
		}

		DebugNoteBillboard nearestNote = null;
		float nearestNoteDistance = 0;
		foreach (DebugNoteBillboard noteBillboard in debugNoteBillboards)
		{
			float noteDistance = Vector3.Distance(playerCharacterController.transform.position, noteBillboard.transform.position);
			if (noteDistance <= maxFocusDistance)
			{
				if (nearestNote == null)
				{
					nearestNote = noteBillboard;
					nearestNoteDistance = noteDistance;

					continue;
				}

				if (noteDistance < nearestNoteDistance)
				{
					nearestNote = noteBillboard;
				}
			}
		}

		if (nearestNote)
		{
			if (!focusedNoteBillboard)
			{
				focusedNoteBillboard = nearestNote;
				nearestNote.SetFocused(true);
			}
			else if (nearestNote != focusedNoteBillboard)
			{
				focusedNoteBillboard.SetFocused(false);
				focusedNoteBillboard = nearestNote;
				nearestNote.SetFocused(true);
			}
		}
		else
		{
			if (focusedNoteBillboard)
			{
				focusedNoteBillboard.SetFocused(false);
				focusedNoteBillboard = null;
			}
		}
	}

	private void PlaceDebugNote(DebugNoteData noteData)
	{
		DebugNoteBillboard newDebugNoteBillboard = Instantiate(debugNoteBillboardPrefab, noteData.position, Quaternion.identity, transform);
		debugNoteBillboards.Add(newDebugNoteBillboard);
		newDebugNoteBillboard.ApplyNoteData(noteData);
	}

	private void PlaceStoredDebugNotes()
	{
		foreach (DebugNoteData note in debugNotesStorage.notes)
		{
			PlaceDebugNote(note);
		}
	}

	public void CreateNewNote(string title, string note, Vector3 position)
	{
		DebugNoteData newNoteData = new()
		{
			title = title,
			note = note,
			position = position
		};

		debugNotesStorage.notes.Add(newNoteData);

		PlaceDebugNote(newNoteData);
	}

	public void RegisterPlayerCharacterController(PlayerCharacterController newPlayerCharacterController)
	{
		playerCharacterController = newPlayerCharacterController;
	}

	public void HidePlaceNoteScreen()
	{
		placeNoteScreen.SetActive(false);

		newNoteTitleInputField.text = "";
		newNoteNoteInputField.text = "";

		playerCharacterController.Frozen = false;
	}
	public void ShowPlaceNoteScreen()
	{
		placeNoteScreen.SetActive(true);
		playerCharacterController.Frozen = true;
	}

	public void OnPlaceNewNotePressed()
	{
		if (playerCharacterController == null) return;
		if (newNoteTitleInputField.text.Length <= 0) return;

		CreateNewNote(newNoteTitleInputField.text, newNoteNoteInputField.text, playerCharacterController.transform.position + notePlacePositionOffset);
		HidePlaceNoteScreen();
	}

	private void EnableInput()
	{
		debugNotesActions = GameInputManager.Instance.gameInputSystem.DebugNotes;

		debugNotesActions.SetCallbacks(this);
		debugNotesActions.Enable();
	}

	public void OnOpenPlaceNoteScreen(InputAction.CallbackContext context)
	{
		ShowPlaceNoteScreen();
	}
}
