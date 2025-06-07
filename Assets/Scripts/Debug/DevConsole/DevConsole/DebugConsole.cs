using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


namespace DigDig2.Debug
{
    public class DebugConsole : Singleton<DebugConsole>
    {
        [SerializeField] private string prefix = string.Empty;
        [SerializeField, TextArea] private string standardSuggestion;
        [SerializeField] private ConsoleCommand[] commands;
        [SerializeField] KeyCode cycleKey;
        [SerializeField] KeyCode confirmKey;

        [Header("UI")]
        [SerializeField] private GameObject backgroundImage;
        [SerializeField] private GameObject oneLineCanvas = null;
        [SerializeField] private TMP_InputField oneLineInputField = null;
        [SerializeField] private TMP_Text oneLineSuggestionText;

        [SerializeField] private GameObject historyLineCanvas = null;
        [SerializeField] private TMP_InputField historyLineInputField = null;
        [SerializeField] private TMP_Text historyLineSuggestionText;

        [SerializeField] private GameObject historyItemPrefab; 
        [SerializeField] private Transform historyContent; // Content object of the Scroll View

        private TMP_InputField inputField = null;
        private TMP_Text suggestionText = null;

        private float pausedTimeScale;

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
        private List<string> commandHistory = new List<string>();
        private int historyIndex = -1;

        // Highlighted text
        private string highlightedText = string.Empty;

        enum ConsoleStates
        {
            None,
            OneLine,
            History,
        }

        ConsoleStates currentConsoleState = ConsoleStates.None;

        private GameInputSystem.DebugConsoleActions inputMap;

        private void Start()
        {
            inputMap = GameInputManager.Instance.gameInputSystem.DebugConsole;
            inputMap.OpenDebugConsole.performed += context => CycleDevConsole();
            inputMap.CloseDebugConsole.performed += context => CloseConsole();
            inputMap.Confirm.started += context => OnConfirm();
            inputMap.CycleSuggestions.started += OnCycleSuggestions;
            inputMap.HistoryNavigation.started += context =>
            {
                if (currentConsoleState == ConsoleStates.None) return;
                if (context.ReadValue<float>() > 0) HandleHistoryNavigationUp();
                else HandleHistoryNavigationDown();
            };
            inputMap.Enable();
        }

        private void OnDisable()
        {
            inputMap.Disable();
        }

        private void Update()
        {

        }

        void OnConfirm()
        {
            if (currentConsoleState == ConsoleStates.None) return;

            ProcessCommand(currentSuggestionIndex != -1 ? currentSuggestions[currentSuggestionIndex] : inputField.text);
            currentSuggestionIndex = -1;
        }

        void OnCycleSuggestions(InputAction.CallbackContext context)
        {
            if (currentConsoleState == ConsoleStates.None) return;

            if (context.started)
            {
                CycleTabCompletion();
            }
        }

        void CycleDevConsole()
        {
            int consoleStatesCount = Enum.GetValues(typeof(ConsoleStates)).Length;

            // Cycle through the console states
            currentConsoleState = (ConsoleStates)(((int)currentConsoleState + 1) % consoleStatesCount);

            if (currentConsoleState == ConsoleStates.None)
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
            currentConsoleState = ConsoleStates.None;
            UpdateUI();
            inputField = null;
            suggestionText = null;
            commandHistory.Clear();
            historyIndex = -1;
            currentSuggestionIndex = -1;
            currentSuggestions.Clear();
        }

        void SetPlayerActive(bool active)
        {
            
        }

        private void UpdateUI()
        {
            switch (currentConsoleState)
            {
                case ConsoleStates.None:
                    oneLineCanvas.SetActive(false);
                    historyLineCanvas.SetActive(false);
                    backgroundImage.SetActive(false);
                    historyLineInputField.text = "";
                    oneLineInputField.text = "";
                    break;

                case ConsoleStates.OneLine:
                    oneLineCanvas.SetActive(true);
                    historyLineCanvas.SetActive(false);
                    backgroundImage.SetActive(true);

                    // Transfer text
                    oneLineInputField.text = historyLineInputField.text;

                    // Set focus to the input field
                    inputField = oneLineInputField;
                    suggestionText = oneLineSuggestionText;
                    StartCoroutine(ActivateInputField(inputField));
                    break;

                case ConsoleStates.History:
                    oneLineCanvas.SetActive(false);
                    historyLineCanvas.SetActive(true);
                    backgroundImage.SetActive(true);

                    // Transfer text
                    historyLineInputField.text = oneLineInputField.text;

                    // Set focus to the input field
                    inputField = historyLineInputField;
                    suggestionText = historyLineSuggestionText;
                    StartCoroutine(ActivateInputField(inputField));
                    UpdateHistoryUI(); // Update the history UI
                    break;
            }
        }

        private IEnumerator ActivateInputField(TMP_InputField inputField)
        {
            yield return null; 
            inputField.ActivateInputField();
            inputField.caretPosition = inputField.text.Length; // Set caret to the end of the text
        }

        // Called by Unity's InputField
        int previousTextLength = 0;
        public void TextChanged(string text)
        {
            if (text.Length > previousTextLength) NewLetterTyped(text[text.Length-1]);
            previousTextLength = text.Length;

            var parts = text.Split(' ');

            currentSuggestions = commandSystem.GetSuggestions(text);

            currentSuggestionIndex = -1;
            UpdateSuggestionText();
        }

        public void ProcessCommand(string inputValue)
        {
            if (!string.IsNullOrWhiteSpace(inputValue))
            {
                // Add the command to history
                commandHistory.Add(inputValue);
                historyIndex = commandHistory.Count; // Reset history index
                UpdateHistoryUI();
            }

            if (CommandSystem.ProcessCommand(inputValue))
            {
                
            }

            inputField.text = string.Empty;
            FocusOnInputField();
        }


        private void CycleTabCompletion()
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
            if (currentSuggestions.Count == 0)
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
                    suggestions += $"<b><color=yellow>{prefix}{currentSuggestions[i]}</color></b>\n";
                }
                else
                {
                    suggestions += $"{prefix}{currentSuggestions[i]}\n";
                }
            }

            suggestionText.text = suggestions;
        }

        private void HandleHistoryNavigationUp()
        {
            if (commandHistory.Count == 0) return;

            // Move up in history (decrement index)
            historyIndex = Mathf.Clamp(historyIndex - 1, 0, commandHistory.Count - 1);
            inputField.text = commandHistory[historyIndex];
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

        private void UpdateHistoryUI()
        {
            // Clear existing history items
            foreach (Transform child in historyContent)
            {
                Destroy(child.gameObject);
            }

            // Create new history items
            foreach (string command in commandHistory)
            {
                GameObject historyItem = Instantiate(historyItemPrefab, historyContent);
                TMP_Text textComponent = historyItem.GetComponentInChildren<TMP_Text>();
                textComponent.text = command;
            }
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
            if (c == char.Parse(" "))
            {
                inputField.SetTextWithoutNotify(currentSuggestionIndex != -1 ? prefix + currentSuggestions[currentSuggestionIndex] + " " : inputField.text);
                inputField.caretPosition = inputField.text.Length;  
            }
        }


    }
}