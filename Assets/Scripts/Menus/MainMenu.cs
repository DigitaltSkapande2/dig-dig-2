using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace DigDig2
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenu : MonoBehaviour
    {
        private const string NAVIGATION_START_CLASS_NAME = "navigationStart";

        private UIDocument uiDocument;

        private string selectedNavigationItem = "";
        private VisualElement mainNavigationContainer;
        private Button continueButton;
        private Button loadGameButton;
        private Button newGameButton;
        private Button settingsButton;
        private Button quitButton;

        private VisualElement saveMenu;
        private VisualElement settingsMenu;



        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }
        private void Start()
        {
            mainNavigationContainer = uiDocument.rootVisualElement.Query<VisualElement>("mainNavigation");

            continueButton = uiDocument.rootVisualElement.Query<Button>("continue");
            continueButton.clicked += OnContinue;

            loadGameButton = uiDocument.rootVisualElement.Query<Button>("loadGame");
            loadGameButton.clicked += OnLoadGame;

            newGameButton = uiDocument.rootVisualElement.Query<Button>("newGame");
            newGameButton.clicked += OnNewGame;

            settingsButton = uiDocument.rootVisualElement.Query<Button>("settings");
            settingsButton.clicked += OnSettings;

            quitButton = uiDocument.rootVisualElement.Query<Button>("quit");
            quitButton.clicked += OnQuit;

            saveMenu = uiDocument.rootVisualElement.Query<VisualElement>("saveMenu");
            saveMenu.visible = false;
            settingsMenu = uiDocument.rootVisualElement.Query<VisualElement>("settingsMenu");
            settingsMenu.visible = false;

            SelectNavigationItem("");
        }

        private void SelectNavigationItem(string itemName)
        {
            string lastSelectedNavigationItem = selectedNavigationItem;
            if (selectedNavigationItem == itemName) selectedNavigationItem = "";
            else selectedNavigationItem = itemName;

            saveMenu.style.translate = new Translate(50f, 0f);
            saveMenu.style.opacity = 0f;
            saveMenu.enabledSelf = false;

            settingsMenu.style.translate = new Translate(50f, 0f);
            settingsMenu.style.opacity = 0f;
            settingsMenu.enabledSelf = false;

            VisualElement elementToBeFocused = null;
            switch (selectedNavigationItem)
            {
                case "loadGame":
                    saveMenu.visible = true;
                    saveMenu.style.translate = new Translate(0f, 0f);
                    saveMenu.style.opacity = 1f;
                    saveMenu.enabledSelf = true;

                    elementToBeFocused = saveMenu.Query<VisualElement>(null, NAVIGATION_START_CLASS_NAME).First();

                    break;
                case "settings":
                    settingsMenu.visible = true;
                    settingsMenu.style.translate = new Translate(0f, 0f);
                    settingsMenu.style.opacity = 1f;
                    settingsMenu.enabledSelf = true;

                    elementToBeFocused = settingsMenu.Query<VisualElement>(null, NAVIGATION_START_CLASS_NAME).First();

                    break;
            }

            mainNavigationContainer.enabledSelf = selectedNavigationItem == "";
            mainNavigationContainer.style.translate = mainNavigationContainer.enabledSelf ? new Translate(0f, 0f) : new Translate(-50f, 0f);
            mainNavigationContainer.style.opacity = mainNavigationContainer.enabledSelf ? 1f : 0.5f;

            if (selectedNavigationItem != "") uiDocument.rootVisualElement.Query<Button>(selectedNavigationItem).First().AddToClassList("selected");
            if (lastSelectedNavigationItem != "") uiDocument.rootVisualElement.Query<Button>(lastSelectedNavigationItem).First().RemoveFromClassList("selected");

            if (selectedNavigationItem == "")
            {
                if (lastSelectedNavigationItem != "") uiDocument.rootVisualElement.Query<Button>(lastSelectedNavigationItem).First().Focus();
                else continueButton.Focus();
            }

            elementToBeFocused?.Focus();
        }

        private void OnCancel()
        {
            SelectNavigationItem("");
        }

        private void OnContinue()
        {
            SelectNavigationItem("continue");
        }
        private void OnLoadGame()
        {
            SelectNavigationItem("loadGame");
        }
        private void OnNewGame()
        {
            SelectNavigationItem("newGame");
        }
        private void OnSettings()
        {
            SelectNavigationItem("settings");
        }
        private void OnQuit()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
            #if UNITY_STANDALONE
                Application.Quit();
            #endif
        }
    }
}
