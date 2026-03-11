using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DigDig2.Input;
using DigDig2.Util;

using TMPro;

using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2.Debugging.Console
{
	public class DebugConsole : Singleton<DebugConsole>, ProjectWideInputActions.IDebugConsoleActions
	{
		[SerializeField] [TextArea] private string standardSuggestion;
		[SerializeField] private ConsoleCommand[ ] commands;

		[Header( "UI" )]
		[SerializeField] private GameObject canvas;

		[SerializeField] private TMP_InputField inputField;
		[SerializeField] private TMP_Text suggestionText;
		[SerializeField] private GameObject consoleMessagePrefab;
		[SerializeField] private Transform consoleMessageParent;

		// Command history
		private readonly List<CommandDescriptor> commandHistory = new( );

		private CommandSystem commandSystem;

		private CommandDescriptor currentlyEnteredCommand;

		private int currentSuggestionIndex = -1;
		private List<string> currentSuggestions = new( );
		private int historyIndex = -1;

		// Input handling
		private ProjectWideInputActions.DebugConsoleActions inputMap;

		// Console state management
		private bool isConsoleOpen;

		private CommandSystem CommandSystem
		{
			get
			{
				if ( commandSystem != null ) return commandSystem;

				return commandSystem = new( commands );
			}
		}

		private IEnumerator ActivateInputField( TMP_InputField activatedInputField )
		{
			yield return null;

			activatedInputField.ActivateInputField( );
			activatedInputField.caretPosition = activatedInputField.text.Length; // Set caret to the end of the text
		}

		private async void ProcessCommand( CommandDescriptor commandToProcess )
		{
			// Add the command to history
			commandHistory.Add( commandToProcess );
			historyIndex = commandHistory.Count; // Reset history index
			PrintConsoleMessage( commandToProcess.ToString( ) );
			inputField.text = string.Empty;
			inputField.interactable = false;

			// Asynchronously Process the command
			CommandResultContext result = await CommandSystem.ProcessCommand( commandToProcess );
			switch ( result.type )
			{
				case CommandResultContext.Type.Error when result.message != null: PrintConsoleMessage( $"<color=#AA0000>Error</color>: {result.message}" ); break;
				case CommandResultContext.Type.Error: PrintConsoleMessage( $"<color=#AA0000>Error</color>: Unspecified error while executing command {commandToProcess.ToString( )}" ); break;
				case CommandResultContext.Type.Warning: PrintConsoleMessage( $"<color=#AAAA00>Warning</color>: {result}" ); break;
				case CommandResultContext.Type.Success: break;
				default: throw new ArgumentOutOfRangeException( );
			}

			inputField.interactable = true;
			FocusOnInputField( );
		}

		private void HandleHistoryNavigationUp( )
		{
			if ( commandHistory.Count == 0 ) return;

			// Move up in history (decrement index)
			historyIndex = Mathf.Clamp( historyIndex - 1, 0, commandHistory.Count - 1 );
			inputField.text = commandHistory[ historyIndex ].ToString( );
			inputField.caretPosition = inputField.text.Length;
		}

		private void HandleHistoryNavigationDown( )
		{
			if ( commandHistory.Count == 0 ) return;

			// Move down in history (increment index)
			historyIndex = Mathf.Clamp( historyIndex + 1, 0, commandHistory.Count - 1 );
			inputField.text = historyIndex < commandHistory.Count ? commandHistory[ historyIndex ] : string.Empty;
			inputField.caretPosition = inputField.text.Length;
		}

		private void FocusOnInputField( )
		{
			if ( inputField != null ) inputField.ActivateInputField( );
		}

		private void NewLetterTyped( char c )
		{
			Debug.Log( $"new letter typed: {c}" );
			if ( c == char.Parse( " " ) ) InsertCurrentlySelectedSuggestion( );
		}

		#region Unity Messages

		private void Start( )
		{
			inputMap = InputManager.Instance.inputActions.DebugConsole;
			inputMap.SetCallbacks( this );
		}

		private void OnDisable( ) { inputMap.RemoveCallbacks( this ); }

		#endregion

		#region Public Methods

		private void PrintConsoleMessage( string message )
		{
			GameObject consoleMessage = Instantiate( consoleMessagePrefab, consoleMessageParent );
			TMP_Text messageText = consoleMessage.GetComponentInChildren<TMP_Text>( );
			if ( messageText )
				messageText.text = message;
			else
				Debug.LogWarning( "Console message prefab does not have a TMP_Text component." );
		}

		// Called by Unity's InputField
		private readonly int previousTextLength = 0;

		public void TextChanged( string text )
		{
			Debug.Log( "---------Text changed--------" );
			if ( !string.IsNullOrEmpty( text ) && text.Length > previousTextLength ) NewLetterTyped( text[ inputField.caretPosition - 1 ] );

			currentlyEnteredCommand = new( inputField.text );
			if ( !currentlyEnteredCommand.IsValid ) return;

			Debug.Log( $"Command Descriptor: CommandWord: {currentlyEnteredCommand.commandWord}, Args: {string.Join( ", ", currentlyEnteredCommand.args )}" );

			// Update Suggestions
			UpdateSuggestions( );
		}

		#endregion

		#region Input Handling

		public void OnConfirm( InputAction.CallbackContext context )
		{
			if ( !context.performed ) return;
			if ( !isConsoleOpen || inputField.text == string.Empty ) return;

			if ( currentSuggestionIndex != -1 && currentSuggestions.Count > 0 )
			{
				// If a suggestion is selected, insert it into the input field
				InsertCurrentlySelectedSuggestion( );
			} else
			{
				// Process the command

				ProcessCommand( new( inputField.text ) );
			}
		}

		public void OnCycleSuggestions( InputAction.CallbackContext context )
		{
			if ( !isConsoleOpen ) return;

			if ( context.started ) CycleSuggestionSelection( );
		}

		public void OnOpenDebugConsole( InputAction.CallbackContext context )
		{
			if ( context.performed ) ToggleDevConsole( );
		}

		public void OnCloseDebugConsole( InputAction.CallbackContext context )
		{
			if ( context.performed ) CloseConsole( );
		}

		public void OnHistoryNavigation( InputAction.CallbackContext context )
		{
			if ( !isConsoleOpen ) return;

			if ( context.ReadValue<float>( ) > 0 )
				HandleHistoryNavigationUp( );
			else
				HandleHistoryNavigationDown( );
		}

		#endregion

		#region Suggestion Handling

		private void UpdateSuggestions( )
		{
			if ( !isConsoleOpen || !inputField ) return;

			if ( !currentlyEnteredCommand.IsValid )
				currentSuggestions.Clear( );
			else
				currentSuggestions = CommandSystem.GetSuggestions( currentlyEnteredCommand );

			// Get suggestions from the command system
			currentSuggestionIndex = -1;
			UpdateSuggestionText( );
		}

		private void InsertCurrentlySelectedSuggestion( )
		{
			if ( currentSuggestionIndex == -1 || currentSuggestions.Count == 0 ) return;

			// Insert the currently selected suggestion into the input field
			string selectedSuggestion = currentSuggestions[ currentSuggestionIndex ];
			string[ ] parts = currentlyEnteredCommand.GetFullCommand( );

			// Reconstruct the input field text
			string newText = "";
			if ( parts.Length > 1 ) newText = string.Join( " ", parts.Take( parts.Length - 1 ) ) + " ";
			newText += selectedSuggestion + " ";

			inputField.SetTextWithoutNotify( newText );
			currentlyEnteredCommand = new( newText ); // Update the command descriptor
			inputField.caretPosition = inputField.text.Length; // Move caret to the end
			UpdateSuggestions( );

			inputField.ActivateInputField( );
		}

		private void CycleSuggestionSelection( )
		{
			if ( currentSuggestions.Count == 0 ) return;

			// UnityEngine.Debug.Log("Cycling through suggestions");

			// Increment suggestion index
			currentSuggestionIndex = ( currentSuggestionIndex + 1 ) % currentSuggestions.Count;

			// Redraw the suggestion list with the current index highlighted
			UpdateSuggestionText( );
		}

		private void UpdateSuggestionText( )
		{
			if ( currentSuggestions == null || currentSuggestions.Count == 0 )
			{
				suggestionText.text = standardSuggestion;
				return;
			}

			// Build the suggestion list with the highlighted suggestion
			string suggestions = "";
			for ( int i = 0; i < currentSuggestions.Count; i++ )
			{
				if ( i == currentSuggestionIndex )
				{
					// Highlight the currently selected suggestion (using bold or color)
					suggestions += $"<color=yellow>{currentSuggestions[ i ]}</color>\n";
				} else
					suggestions += $"{currentSuggestions[ i ]}\n";
			}

			suggestionText.text = suggestions;
		}

		#endregion

		#region State Management

		private void ToggleDevConsole( )
		{
			isConsoleOpen = !isConsoleOpen;

			if ( !isConsoleOpen )
			{
				CloseConsole( );
				return;
			}

			UpdateUI( );

			FocusOnInputField( );
		}

		private void CloseConsole( )
		{
			isConsoleOpen = false;
			UpdateUI( );
			commandHistory.Clear( );
			historyIndex = -1;
			currentSuggestionIndex = -1;
			currentSuggestions.Clear( );
		}

		private void UpdateUI( )
		{
			if ( isConsoleOpen )
			{
				canvas.SetActive( true );
				StartCoroutine( ActivateInputField( inputField ) ); // Activate the input field after the UI is updated
			} else
			{
				canvas.SetActive( false );
				inputField.text = "";
				suggestionText.text = "";
			}
		}

		#endregion
	}
}
