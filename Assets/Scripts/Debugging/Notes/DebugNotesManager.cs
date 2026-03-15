using System;
using System.Collections.Generic;

using DigDig2.Game;
using DigDig2.Util;

using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DigDig2.Debugging.Notes
{
	public class DebugNotesManager : Singleton<DebugNotesManager>
	{
		[Header( "Note Storage" )]
		[Tooltip( "DebugNotesStorage scriptable object used to store note data." )]
		[SerializeField] private DebugNotesStorage debugNotesStorage;

		[Header( "Interaction" )]
		[Tooltip( "Distance from a note the player has to be for it to become focused." )]
		[SerializeField] private float maxFocusDistance = 5f;

		[Tooltip( "The offset from the player's position where the note gets placed." )]
		[SerializeField] private Vector3 notePlacePositionOffset = new( 0f, 1f, 0f );

		[Header( "Note Management Interface" )]
		[Tooltip( "The note management interface's title." )]
		[SerializeField] private TMP_Text interfaceTitle;

		[Tooltip( "The note management interface's title input field." )]
		[SerializeField] private TMP_InputField interfaceTitleInputField;

		[Tooltip( "The note management interface's note input field." )]
		[SerializeField] private TMP_InputField interfaceNoteInputField;

		[Tooltip( "The note management interface's author input field." )]
		[SerializeField] private TMP_InputField interfaceAuthorInputField;

		[Tooltip( "The note management interface's archive button" )]
		[SerializeField] private Button interfaceArchiveButton;

		[Tooltip( "The note management interface's canvas." )]
		[SerializeField] private GameObject interfaceCanvas;

		[Header( "Prefabs" )]
		[Tooltip( "The in-game note billboard prefab." )]
		[SerializeField] private DebugNoteBillboard debugNoteBillboardPrefab;

		private readonly Dictionary<int, DebugNoteBillboard> archivedDebugNoteBillboards = new( );

		private readonly Dictionary<int, DebugNoteBillboard> debugNoteBillboards = new( );
		private int editingNoteIndex = -1;

		private int focusedNoteIndex = -1;
		private NoteManagementMode noteManagementMode = NoteManagementMode.None;
		private bool showArchivedDebugNotes;
		private bool showDebugNotes = true;

		public bool ShowDebugNotes
		{
			get => showDebugNotes;

			set
			{
				showDebugNotes = value;
				RefreshNoteBillboardVisibility( );
			}
		}

		public bool ShowArchivedDebugNotes
		{
			get => showArchivedDebugNotes;

			set
			{
				showArchivedDebugNotes = value;
				RefreshNoteBillboardVisibility( );
			}
		}

		private void Start( )
		{
			PlaceStoredDebugNotes( );

			HideNoteManagementInterface( );
		}

		private void Update( )
		{
			// Maybe not too performance friendly to check all notes in a for loop every frame, but uhhhhhhhhhhhhh i don't know any easier way :)
			FocusNearestNote( );
		}

		private enum NoteManagementMode { None, Place, Edit }

		#region Notes Backend

		private DebugNoteScene GetCurrentSceneNoteStorage( )
		{
			if ( debugNotesStorage.scenes.TryGetValue( SceneManager.GetActiveScene( ).path, out DebugNoteScene storage ) ) return storage;

			DebugNoteScene newDebugNoteScene = new( );
			debugNotesStorage.scenes[ SceneManager.GetActiveScene( ).path ] = newDebugNoteScene;
			return newDebugNoteScene;
		}

		// Get the nearest note and focus it, if a note is already focused then unfocus the old one
		private void FocusNearestNote( )
		{
			if ( !GameManager.Instance.PlayerOne.characterObject )
			{
				if ( focusedNoteIndex == -1 ) return;

				DebugNoteBillboard focusedNoteBillboard = GetNoteBillboardFromNoteIndex( focusedNoteIndex );

				focusedNoteBillboard.SetFocused( false );
				focusedNoteIndex = -1;

				return;
			}

			DebugNoteBillboard nearestNoteBillboard = null;
			int nearestNote = -1;
			float nearestNoteDistance = 0;
			foreach ( int noteIndex in debugNoteBillboards.Keys )
			{
				DebugNoteBillboard noteBillboard = GetNoteBillboardFromNoteIndex( noteIndex );
				if ( !noteBillboard.gameObject.activeSelf ) continue;

				float noteDistance = Vector3.Distance( GameManager.Instance.PlayerOne.characterObject.transform.position, noteBillboard.transform.position );
				if ( !( noteDistance <= maxFocusDistance ) ) continue;

				if ( nearestNote == -1 )
				{
					nearestNoteBillboard = noteBillboard;
					nearestNote = noteIndex;
					nearestNoteDistance = noteDistance;

					continue;
				}

				if ( !( noteDistance < nearestNoteDistance ) ) continue;

				nearestNoteBillboard = noteBillboard;
				nearestNote = noteIndex;
			}

			if ( nearestNote != -1 )
			{
				if ( focusedNoteIndex != -1 )
				{
					DebugNoteBillboard focusedNoteBillboard = GetNoteBillboardFromNoteIndex( focusedNoteIndex );

					focusedNoteBillboard.SetFocused( false );
				}

				focusedNoteIndex = nearestNote;
				nearestNoteBillboard?.SetFocused( true );
			}
			else
			{
				if ( focusedNoteIndex == -1 ) return;

				DebugNoteBillboard focusedNoteBillboard = GetNoteBillboardFromNoteIndex( focusedNoteIndex );

				focusedNoteBillboard.SetFocused( false );
				focusedNoteIndex = -1;
			}
		}

		private void RefreshNoteBillboardVisibility( )
		{
			foreach ( int noteIndex in debugNoteBillboards.Keys )
			{
				DebugNoteBillboard noteBillboard = GetNoteBillboardFromNoteIndex( noteIndex );
				if ( !showDebugNotes )
				{
					noteBillboard.gameObject.SetActive( false );
					continue;
				}

				DebugNoteData noteData = GetNoteDataFromNoteIndex( noteIndex );
				if ( !showArchivedDebugNotes && noteData.archived )
				{
					noteBillboard.gameObject.SetActive( false );
					continue;
				}

				noteBillboard.gameObject.SetActive( true );
			}
		}

		private DebugNoteBillboard GetNoteBillboardFromNoteIndex( int noteIndex ) => debugNoteBillboards.GetValueOrDefault( noteIndex );

		private DebugNoteData GetNoteDataFromNoteIndex( int noteIndex )
		{
			DebugNoteScene currentDebugNoteSceneStorage = GetCurrentSceneNoteStorage( );
			return currentDebugNoteSceneStorage.notes.Count > noteIndex ? currentDebugNoteSceneStorage.notes[ noteIndex ] : null;
		}

		// Place an existing note in the world, does not save a new note to the storage
		private void PlaceStoredDebugNote( int noteIndex )
		{
			DebugNoteData noteData = GetNoteDataFromNoteIndex( noteIndex );

			DebugNoteBillboard newNoteBillboard = Instantiate( debugNoteBillboardPrefab, noteData.position, Quaternion.identity, transform );
			newNoteBillboard.ApplyNoteData( noteData );

			if ( noteData.archived )
				newNoteBillboard.gameObject.SetActive( showDebugNotes && showArchivedDebugNotes );
			else
				newNoteBillboard.gameObject.SetActive( showDebugNotes );

			debugNoteBillboards.Add( noteIndex, newNoteBillboard );
		}

		// Go through the stored notes and place them in the world
		private void PlaceStoredDebugNotes( )
		{
			for ( int noteIndex = 0; noteIndex < GetCurrentSceneNoteStorage( ).notes.Count; noteIndex++ ) { PlaceStoredDebugNote( noteIndex ); }
		}

		// Create a new note in the note storage and place it in the world
		public void CreateNewNote( string title, string note, string author, Vector3 position )
		{
			DebugNoteScene currentDebugNoteSceneStorage = GetCurrentSceneNoteStorage( );

			DebugNoteData newNoteData = new( )
			{
				title = title,
				note = note,
				author = author,
				position = position
			};

			currentDebugNoteSceneStorage.notes.Add( newNoteData );
			int newNoteIndex = currentDebugNoteSceneStorage.notes.Count - 1;

			PlaceStoredDebugNote( newNoteIndex );
		}

		public void EditNote( int noteIndex, string title, string note, string author )
		{
			DebugNoteData noteData = GetNoteDataFromNoteIndex( noteIndex );
			DebugNoteBillboard noteBillboard = GetNoteBillboardFromNoteIndex( noteIndex );

			noteData.title = title;
			noteData.note = note;
			noteData.author = author;

			noteBillboard.ApplyNoteData( noteData );
		}

		public void ArchiveNote( int noteIndex )
		{
			DebugNoteData noteData = GetNoteDataFromNoteIndex( noteIndex );
			if ( noteData.archived )
			{
				Debug.LogError( $"Debug note with index \"{noteIndex}\" was requested to be archived but is already archived!" );
				return;
			}

			noteData.archived = true;

			DebugNoteBillboard noteBillboard = GetNoteBillboardFromNoteIndex( noteIndex );
			noteBillboard.ApplyNoteData( noteData );
			noteBillboard.gameObject.SetActive( showDebugNotes && showArchivedDebugNotes );
		}

		public void UnarchiveNote( int archivedNoteIndex )
		{
			DebugNoteData noteData = GetNoteDataFromNoteIndex( archivedNoteIndex );
			if ( !noteData.archived )
			{
				Debug.LogError( $"Archived debug note with index \"{archivedNoteIndex}\" was requested to be unarchived but was not archived!" );
				return;
			}

			noteData.archived = false;

			DebugNoteBillboard noteBillboard = GetNoteBillboardFromNoteIndex( archivedNoteIndex );
			noteBillboard.ApplyNoteData( noteData );
			noteBillboard.gameObject.SetActive( showDebugNotes );
		}

		#endregion

		#region Notes Frontend

		public void StartNotePlacement( )
		{
			if ( noteManagementMode != NoteManagementMode.None ) return;

			ShowNoteManagementInterface( NoteManagementMode.Place );
		}

		public void StartNoteEditing( )
		{
			if ( noteManagementMode != NoteManagementMode.None ) return;
			if ( focusedNoteIndex == -1 ) return;

			editingNoteIndex = focusedNoteIndex;
			DebugNoteData editingNoteData = GetNoteDataFromNoteIndex( editingNoteIndex );

			interfaceArchiveButton.interactable = true;

			interfaceTitleInputField.text = editingNoteData.title;
			interfaceNoteInputField.text = editingNoteData.note;
			interfaceAuthorInputField.text = editingNoteData.author;

			ShowNoteManagementInterface( NoteManagementMode.Edit );
		}

		private void ShowNoteManagementInterface( NoteManagementMode managementMode )
		{
			if ( !GameManager.Instance.PlayerOne.characterObject ) return;

			noteManagementMode = managementMode;
			interfaceTitle.text = managementMode switch
			{
				NoteManagementMode.Place => "Place Note",
				NoteManagementMode.Edit => "Edit Note",
				_ => interfaceTitle.text
			};

			interfaceCanvas.SetActive( true );

			GameManager.Instance.PlayerOne.characterObject.GetComponent<EntityCharacterController>( ).Frozen = true;
		}

		private void HideNoteManagementInterface( )
		{
			if ( !GameManager.Instance.PlayerOne.characterObject ) return;

			interfaceCanvas.SetActive( false );

			GameManager.Instance.PlayerOne.characterObject.GetComponent<EntityCharacterController>( ).Frozen = false;

			interfaceArchiveButton.interactable = false;

			interfaceTitleInputField.text = "";
			interfaceNoteInputField.text = "";
			interfaceAuthorInputField.text = "";

			noteManagementMode = NoteManagementMode.None;
		}

		// Player pressed the confirm button on the note management interface
		public void OnNoteConfirmPressed( )
		{
			if ( !GameManager.Instance.PlayerOne.characterObject ) return;
			if ( interfaceTitleInputField.text.Length <= 0 ) return;

			switch ( noteManagementMode )
			{
				case NoteManagementMode.Place:
					CreateNewNote(
						interfaceTitleInputField.text,
						interfaceNoteInputField.text,
						interfaceAuthorInputField.text,
						GameManager.Instance.PlayerOne.characterObject.transform.position + notePlacePositionOffset
					); break;
				case NoteManagementMode.Edit:
					EditNote(
						editingNoteIndex,
						interfaceTitleInputField.text,
						interfaceNoteInputField.text,
						interfaceAuthorInputField.text
					); break;
				case NoteManagementMode.None: break;
				default: throw new ArgumentOutOfRangeException( );
			}

			HideNoteManagementInterface( );
		}

		public void OnNoteArchivePressed( )
		{
			if ( editingNoteIndex == -1 ) return;

			DebugNoteData editingNoteData = GetNoteDataFromNoteIndex( editingNoteIndex );

			if ( editingNoteData.archived )
				UnarchiveNote( editingNoteIndex );
			else
				ArchiveNote( editingNoteIndex );

			HideNoteManagementInterface( );
		}

		public void OnNoteCancelPressed( ) { HideNoteManagementInterface( ); }

		#endregion
	}
}
