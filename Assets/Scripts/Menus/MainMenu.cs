using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DigDig2.UIElements;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

namespace DigDig2
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenu : MonoBehaviour
    {
        private const string NAVIGATION_START_CLASS_NAME = "navigationStart";

        [SerializeField] private string gameSceneName = "GameScene";

        private enum HostingModeSelectionType {
            None,

            ContinuedGameSave,
            LoadedGameSave,
            NewGameSave
        }

        private UIDocument uiDocument;

        private string selectedNavigationItem = "";
        private VisualElement mainNavigationContainer;
        private Button continueButton;
        private Button loadGameButton;
        private Button newGameButton;
        private Button settingsButton;
        private Button quitButton;

        private VisualElement saveMenu;
        private List<string> saveFiles;
        private ListView saveFileList;
        private int selectedSaveFileIndex = -1;
        private VisualElement saveManagement;
        private Label saveManagementSaveName;
        private Button saveLoadButton;
        private Button saveRenameButton;
        private Button saveDeleteButton;
        private int loadedSaveFileIndex = -1;

        private VisualElement settingsMenu;

        private VisualElement hostingScreen;
        private VisualElement modeSelectionAnimationContainer;
        private HostingModeSelectionType hostingModeSelectionType = HostingModeSelectionType.None;
        private Button singleplayerModeButton;
        private Button multiplayerModeButton;



        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }
        private void Start()
        {
            saveFiles = SaveManager.Instance.GetSaveFiles();

            mainNavigationContainer = uiDocument.rootVisualElement.Query<VisualElement>("mainNavigation");

            continueButton = mainNavigationContainer.Query<Button>("continue");
            continueButton.clicked += OnContinue;

            loadGameButton = mainNavigationContainer.Query<Button>("loadGame");
            loadGameButton.clicked += OnLoadGame;

            newGameButton = mainNavigationContainer.Query<Button>("newGame");
            newGameButton.clicked += OnNewGame;

            settingsButton = mainNavigationContainer.Query<Button>("settings");
            settingsButton.clicked += OnSettings;

            quitButton = mainNavigationContainer.Query<Button>("quit");
            quitButton.clicked += OnQuit;

            // Save Menu
            saveMenu = uiDocument.rootVisualElement.Query<VisualElement>("saveMenu");
            saveMenu.visible = false;
            
            saveFileList = saveMenu.Query<ListView>("saveFileList");
            saveFileList.makeItem = () =>
            {
                return new SaveFile();
            };
            saveFileList.bindItem = (element, index) =>
            {
                SaveFile saveFileElement = (SaveFile)element;
                string saveFilePath = saveFiles[index];
                SaveManager.GameSave gameSave = FileSystem.ReadDataFromFile<SaveManager.GameSave>(saveFilePath);
                saveFileElement.saveName = gameSave.saveName;
                saveFileElement.saveInfo = $"Version: {gameSave.version}";
            };
            saveFileList.itemsSource = saveFiles;
            saveFileList.selectionType = SelectionType.Single;
            
            saveManagement = saveMenu.Query<VisualElement>("saveManagement");
            saveManagement.visible = false;

            saveManagementSaveName = saveMenu.Query<Label>("saveName");

            saveLoadButton = saveManagement.Query<Button>("load");
            saveLoadButton.clicked += LoadSelectedSaveFile;

            saveRenameButton = saveManagement.Query<Button>("rename");

            saveDeleteButton = saveManagement.Query<Button>("delete");

            // Settings Menu
            settingsMenu = uiDocument.rootVisualElement.Query<VisualElement>("settingsMenu");
            settingsMenu.visible = false;

            // Hosting Screen
            hostingScreen = uiDocument.rootVisualElement.Query<VisualElement>("hostingScreen");
            hostingScreen.visible = false;

            modeSelectionAnimationContainer = hostingScreen.Query<VisualElement>("animationContainer");

            singleplayerModeButton = modeSelectionAnimationContainer.Query<Button>("singleplayer");
            singleplayerModeButton.clicked += OnSingleplayerChosen;

            multiplayerModeButton = modeSelectionAnimationContainer.Query<Button>("multiplayer");

            SelectNavigationItem("");
            SelectSaveFile(-1);
            StartCoroutine(HideModeSelection());
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

                    if (saveFiles.Count > 0)
                    {
                        saveFileList.selectedIndex = 0;
                        saveFileList.ScrollToItem(saveFileList.selectedIndex);
                    }

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

        private void SelectSaveFile(int saveFileIndex)
        {
            if (saveFiles.Count > 0 && saveFiles.Count > saveFileIndex)
            {
                if (selectedSaveFileIndex >= 0)
                {
                    SaveFile lastSelectedSaveFileElement = (SaveFile)saveFileList.GetRootElementForIndex(selectedSaveFileIndex);
                    lastSelectedSaveFileElement.RemoveFromClassList("selected");
                    lastSelectedSaveFileElement.Focus();
                    saveFileList.selectedIndex = selectedSaveFileIndex;
                }
                
                if (saveFileIndex >= 0)
                {
                    SaveFile saveFileElement = (SaveFile)saveFileList.GetRootElementForIndex(saveFileIndex);
                    saveFileElement.AddToClassList("selected");

                    saveFileList.enabledSelf = false;

                    string saveFilePath = saveFiles[saveFileIndex];
                    SaveManager.GameSave gameSave = FileSystem.ReadDataFromFile<SaveManager.GameSave>(saveFilePath);
                    saveManagementSaveName.text = gameSave.saveName;

                    saveManagement.visible = true;
                    saveManagement.style.translate = new Translate(0f, 0f);
                    saveManagement.style.opacity = 1f;
                    saveManagement.enabledSelf = true;

                    saveLoadButton.Focus();
                }
            }

            if (saveFileIndex < 0)
            {
                saveFileList.enabledSelf = true;

                saveManagement.style.translate = new Translate(50f, 0f);
                saveManagement.style.opacity = 0f;
                saveManagement.enabledSelf = false;
            }
            
            selectedSaveFileIndex = saveFileIndex;
        }
        private void LoadSelectedSaveFile()
        {
            if (selectedSaveFileIndex >= 0)
            {
                loadedSaveFileIndex = selectedSaveFileIndex;
                StartCoroutine(ShowModeSelection(HostingModeSelectionType.LoadedGameSave));
            }
        }

        private IEnumerator ShowModeSelection(HostingModeSelectionType newHostingModeSelectionType = HostingModeSelectionType.None)
        {
            if (newHostingModeSelectionType != HostingModeSelectionType.None)
            {
                hostingModeSelectionType = newHostingModeSelectionType;

                saveManagement.enabledSelf = false;

                hostingScreen.enabledSelf = true;
                hostingScreen.visible = true;
                hostingScreen.style.opacity = 1f;
                
                yield return new WaitForSeconds(0.5f);

                modeSelectionAnimationContainer.enabledSelf = true;
                modeSelectionAnimationContainer.visible = true;
                modeSelectionAnimationContainer.style.opacity = 1f;
                modeSelectionAnimationContainer.style.translate = new Translate(0, 0);
            }
        }

        private IEnumerator HideModeSelection()
        {
            hostingModeSelectionType = HostingModeSelectionType.None;

            modeSelectionAnimationContainer.enabledSelf = false;
            modeSelectionAnimationContainer.style.opacity = 0f;
            modeSelectionAnimationContainer.style.translate = new Translate(0, 50);
            
            yield return new WaitForSeconds(0.2f);

            modeSelectionAnimationContainer.visible = true;

            hostingScreen.enabledSelf = false;
            hostingScreen.style.opacity = 0f;

            saveManagement.enabledSelf = true;

            yield return new WaitForSeconds(0.5f);

            hostingScreen.visible = false;
        }

        private void OnSingleplayerChosen()
        {
            switch (hostingModeSelectionType)
            {
                case HostingModeSelectionType.ContinuedGameSave:
                    Debug.LogWarning("Continuing not implemented yet!");
                    break;
                case HostingModeSelectionType.LoadedGameSave:
                    if (loadedSaveFileIndex >= 0)
                    {
                        string saveFilePath = saveFiles[loadedSaveFileIndex];
                        SaveManager.GameSave gameSave = FileSystem.ReadDataFromFile<SaveManager.GameSave>(saveFilePath);
                        SaveManager.Instance.LoadSave(gameSave);

                        StartSingleplayer();
                    }
                    break;
                case HostingModeSelectionType.NewGameSave:
                    SaveManager.Instance.CreateNewSave();

                    StartSingleplayer();
                    break;
            }
        }
        private void OnMultiplayerChosen()
        {
            if (loadedSaveFileIndex >= 0)
            {
                Debug.LogWarning("Multiplayer is not implemented yet! ANTON LOCK IN!! pwetty pwease...");
            }
        }

        public async void StartSingleplayer()
        {
            NetworkManager.singleton.StartHost(false);

            await UniTask.WaitUntil(() => NetworkServer.active);
            Debug.Log("GANG!!! WE MADE IT!!");

            NetworkManager.singleton.ServerChangeScene(gameSceneName);
        }

        private void OnCancel()
        {
            switch (selectedNavigationItem)
            {
                case "loadGame":
                    if (selectedSaveFileIndex >= 0)
                    {
                        if (loadedSaveFileIndex >= 0)
                        {
                            StopCoroutine(ShowModeSelection());
                            StartCoroutine(HideModeSelection());
                            loadedSaveFileIndex = -1;

                            return;
                        }
                        
                        SelectSaveFile(-1);
                        return;
                    }
                    break;
            }
            SelectNavigationItem("");
        }
        private void OnSubmit()
        {
            switch (selectedNavigationItem)
            {
                case "loadGame":
                    if (selectedSaveFileIndex < 0) SelectSaveFile(saveFileList.selectedIndex);
                    break;
            }
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
            StartCoroutine(ShowModeSelection(HostingModeSelectionType.NewGameSave));
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
