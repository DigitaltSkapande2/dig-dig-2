using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


namespace DigDig2.Debug
{
    public class DebugConsole : Singleton<DebugConsole>
    {
        [SerializeField, TextArea] private string standardSuggestion;
        [SerializeField] private ConsoleCommandBase[] commands;

        [Header("UI")]
        [SerializeField] private GameObject canvas = null;
        [SerializeField] private TMP_InputField inputField = null;
        [SerializeField] private TMP_Text suggestionText;
        [SerializeField] private GameObject consoleMessagePrefab; 
        [SerializeField] private Transform consoleMessageParent;

        CommandDescriptor currentlyEnteredCommand;

        private CommandSystem commandSystem;
        private CommandSystem CommandSystem
        {
            get
            {
                if (commandSystem != null) { return commandSystem; }
                return commandSystem = new CommandSystem(commands);
            }
        }

        private int currentSuggestionIndex = -1;
        private List<string> currentSuggestions = new List<string>();

        // Command history
        private List<CommandDescriptor> commandHistory = new List<CommandDescriptor>();
        private int historyIndex = -1;

        // Console state management
        bool isConsoleOpen = false;

        // Input handling
        private GameInputSystem.DebugConsoleActions inputMap;

        #region UnityMessages

        private void Start()
        {
            inputMap = GameInputManager.Instance.gameInputSystem.DebugConsole;
            inputMap.OpenDebugConsole.performed += context => CycleDevConsole();
            inputMap.CloseDebugConsole.performed += context => CloseConsole();
            inputMap.Confirm.started += context => OnConfirm();
            inputMap.CycleSuggestions.started += OnCycleSuggestions;
            inputMap.HistoryNavigation.started += context =>
            {
                if (!isConsoleOpen) return;
                if (context.ReadValue<float>() > 0) HandleHistoryNavigationUp();
                else HandleHistoryNavigationDown();
            };
            inputMap.Enable();
        }

        private void OnDisable()
        {
            inputMap.Disable();
        }

        #endregion

        #region PublicMethods

        private void PrintConsoleMessage(string message)
        {
            GameObject consoleMessage = Instantiate(consoleMessagePrefab, consoleMessageParent);
            TMP_Text messageText = consoleMessage.GetComponentInChildren<TMP_Text>();
            if (messageText != null)
            {
                messageText.text = message;
            }
            else
            {
                UnityEngine.Debug.LogWarning("Console message prefab does not have a TMP_Text component.");
            }
        }

        // Called by Unity's InputField
        int previousTextLength = 0;
        public void TextChanged(string text)
        {
            UnityEngine.Debug.Log($"---------Text changed--------");
            if (!string.IsNullOrEmpty(text) && text.Length > previousTextLength) NewLetterTyped(text[inputField.caretPosition - 1]);

            currentlyEnteredCommand = new CommandDescriptor(inputField.text);
            if (!currentlyEnteredCommand.IsValid)
            {
                return;
            }
            
            UnityEngine.Debug.Log($"Command Descriptor: CommandWord: {currentlyEnteredCommand.commandWord}, Args: {string.Join(", ", currentlyEnteredCommand.args)}");

            // Update Suggestions
            UpdateSuggestions();
        }

        #endregion

        #region InputHandling

        void OnConfirm()
        {
            if (!isConsoleOpen || inputField.text == string.Empty) return;

            if (currentSuggestionIndex != -1 && currentSuggestions.Count > 0)
            {
                // If a suggestion is selected, insert it into the input field
                InsertCurrentlySelectedSuggestion();
            }
            else
            {
                // Process the command

                ProcessCommand(new CommandDescriptor(inputField.text));
            }
        }


        void OnCycleSuggestions(InputAction.CallbackContext context)
        {
            if (!isConsoleOpen) return;

            if (context.started)
            {
                CycleSuggestionSelection();
            }
        }

        #endregion

        #region SuggestionHandling

        private void UpdateSuggestions()
        {
            if (!isConsoleOpen || inputField == null) return;

            if (!currentlyEnteredCommand.IsValid)
            {
                currentSuggestions.Clear();
            }
            else
            {
                currentSuggestions = CommandSystem.GetSuggestions(currentlyEnteredCommand);
            }

            // Get suggestions from the command system
            currentSuggestionIndex = -1; 
            UpdateSuggestionText();
        }

        private void InsertCurrentlySelectedSuggestion()
        {
            if (currentSuggestionIndex == -1 || currentSuggestions.Count == 0) return;

            // Insert the currently selected suggestion into the input field
            string selectedSuggestion = currentSuggestions[currentSuggestionIndex];
            var parts = currentlyEnteredCommand.GetFullCommand();

            // Reconstruct the input field text
            string newText = "";
            if (parts.Length > 1)
            {
                newText = string.Join(" ", parts.Take(parts.Length - 1)) + " ";
            }
            newText += selectedSuggestion + " ";

            inputField.SetTextWithoutNotify(newText);
            currentlyEnteredCommand = new CommandDescriptor(newText); // Update the command descriptor
            inputField.caretPosition = inputField.text.Length; // Move caret to the end
            UpdateSuggestions(); 

            inputField.ActivateInputField();
        }

        private void CycleSuggestionSelection()
        {
            if (currentSuggestions.Count == 0) return;

            //UnityEngine.Debug.Log("Cycling through suggestions");

            // Increment suggestion index
            currentSuggestionIndex = (currentSuggestionIndex + 1) % currentSuggestions.Count;

            // Redraw the suggestion list with the current index highlighted
            UpdateSuggestionText();
        }

        private void UpdateSuggestionText()
        {
            if (currentSuggestions == default || currentSuggestions.Count == 0)
            {
                suggestionText.text = standardSuggestion;
                return;
            }

            // Build the suggestion list with the highlighted suggestion
            string suggestions = "";
            for (int i = 0; i < currentSuggestions.Count; i++)
            {
                if (i == currentSuggestionIndex)
                {
                    // Highlight the currently selected suggestion (using bold or color)
                    suggestions += $"<color=yellow>{currentSuggestions[i]}</color>\n";
                }
                else
                {
                    suggestions += $"{currentSuggestions[i]}\n";
                }
            }

            suggestionText.text = suggestions;
        }

        #endregion

        #region StateManagement

        void CycleDevConsole()
        {
            // Cycle through the console states
            isConsoleOpen = !isConsoleOpen;

            if (!isConsoleOpen)
            {
                CloseConsole();
                return;
            }

            // Update UI and activate the appropriate input field
            UpdateUI();

            // Ensure that the active input field is focused
            FocusOnInputField();
        }

        void CloseConsole()
        {
            isConsoleOpen = false;
            UpdateUI();
            commandHistory.Clear();
            historyIndex = -1;
            currentSuggestionIndex = -1;
            currentSuggestions.Clear();
        }

        private void UpdateUI()
        {
            if (isConsoleOpen)
            {
                canvas.SetActive(true);
                StartCoroutine(ActivateInputField(inputField)); // Activate the input field after the UI is updated
            }
            else
            {
                canvas.SetActive(false);
                inputField.text = "";
                suggestionText.text = "";
            }
        }
        
        #endregion

        private IEnumerator ActivateInputField(TMP_InputField inputField)
        {
            yield return null;
            inputField.ActivateInputField();
            inputField.caretPosition = inputField.text.Length; // Set caret to the end of the text
        }



        public async void ProcessCommand(CommandDescriptor _commandToProcess)
        {
            // Add the command to history
            commandHistory.Add(_commandToProcess);
            historyIndex = commandHistory.Count; // Reset history index
            PrintConsoleMessage(_commandToProcess.ToString());
            inputField.text = string.Empty;
            inputField.interactable = false;

            // Asyncronously Process the command
            CommandResultContext result = await CommandSystem.ProcessCommand(_commandToProcess);
            if (result.type == CommandResultContext.Type.Error && result.errorMessage != null)
            {
                PrintConsoleMessage($"<color=#AA0000>Error</color>: {result.errorMessage}");
            }
            else if (result.type == CommandResultContext.Type.Error)
            {
                PrintConsoleMessage($"<color=#AA0000>Error</color>: Unspecified error while executing command '{_commandToProcess.ToString()}'");
            }

            inputField.interactable = true;
            FocusOnInputField();
        }


        private void HandleHistoryNavigationUp()
        {
            if (commandHistory.Count == 0) return;

            // Move up in history (decrement index)
            historyIndex = Mathf.Clamp(historyIndex - 1, 0, commandHistory.Count - 1);
            inputField.text = commandHistory[historyIndex].ToString();
            inputField.caretPosition = inputField.text.Length;
        }

        private void HandleHistoryNavigationDown()
        {
            if (commandHistory.Count == 0) return;

            // Move down in history (increment index)
            historyIndex = Mathf.Clamp(historyIndex + 1, 0, commandHistory.Count - 1);
            inputField.text = historyIndex < commandHistory.Count ? commandHistory[historyIndex] : string.Empty;
            inputField.caretPosition = inputField.text.Length;
        }

        private void FocusOnInputField()
        {
            if (inputField != null)
            {
                inputField.ActivateInputField();
            }
        }


        private void NewLetterTyped(char c)
        {
            UnityEngine.Debug.Log($"new letter typed: {c}");
            if (c == char.Parse(" "))
            {
                InsertCurrentlySelectedSuggestion();
            }
        }


    }
}