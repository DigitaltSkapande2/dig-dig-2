using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DigDig2.UIElements;
using DigDig2.UINavigation;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

namespace DigDig2
{
    [RequireComponent(typeof(UIDocument), typeof(UserInterfaceNavigator))]
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameSceneName = "GameScene";

        private enum HostingModeSelectionType {
            None,

            ContinuedGameSave,
            LoadedGameSave,
            NewGameSave
        }

        private UIDocument uiDocument;
        private UserInterfaceNavigator navigator;

        private string selectedNavigationItem = "";
        private VisualElement mainNavigationContainer;
        private Button continueButton;
        private Button loadGameButton;
        private Button newGameButton;
        private Button settingsButton;
        private Button joinGameButton;
        private Button quitButton;

        private VisualElement saveMenu;
        private List<string> saveFiles;
        private SubmittableListView saveFileList;
        private int selectedSaveFileIndex = -1;
        private VisualElement saveManagement;
        private Label saveManagementSaveName;
        private Button saveLoadButton;
        private Button saveRenameButton;
        private Button saveDeleteButton;
        private int loadedSaveFileIndex = -1;

        private VisualElement settingsMenu;

        private VisualElement screenCover;
        private VisualElement hostingSelctionAnimationContainer;
        private HostingModeSelectionType hostingModeSelectionType = HostingModeSelectionType.None;
        private Button singleplayerModeButton;
        private Button multiplayerModeButton;

        private Focusable lastFocus;



        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            navigator = GetComponent<UserInterfaceNavigator>();
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
            
            saveFileList = saveMenu.Query<SubmittableListView>("saveFileList");
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
            saveFileList.submit += SelectListViewSelectedSaveFile;
            
            saveManagement = saveMenu.Query<VisualElement>("saveManagement");

            saveManagementSaveName = saveMenu.Query<Label>("saveName");

            saveLoadButton = saveManagement.Query<Button>("load");
            saveLoadButton.clicked += LoadSelectedSaveFile;

            saveRenameButton = saveManagement.Query<Button>("rename");

            saveDeleteButton = saveManagement.Query<Button>("delete");

            // Settings Menu
            settingsMenu = uiDocument.rootVisualElement.Query<VisualElement>("settingsMenu");

            // Hosting Screen
            screenCover = uiDocument.rootVisualElement.Query<VisualElement>("screenCover");

            hostingSelctionAnimationContainer = screenCover.Query<VisualElement>("animationContainer");

            singleplayerModeButton = hostingSelctionAnimationContainer.Query<Button>("singleplayer");
            singleplayerModeButton.clicked += OnSingleplayerChosen;

            multiplayerModeButton = hostingSelctionAnimationContainer.Query<Button>("multiplayer");
            multiplayerModeButton.clicked += OnMultiplayerChosen;

            saveMenu.RegisterCallback<NavigatorOpenedEvent>((evt) =>
            {
                selectedSaveFileIndex = -1;
                for (int index = 0; index < saveFiles.Count; index++)
                {
                    SaveFile saveFileElement = (SaveFile)saveFileList.GetRootElementForIndex(index);
                    saveFileElement?.RemoveFromClassList("selected");
                }
            });

            saveManagement.RegisterCallback<NavigatorOpenedEvent>((evt) =>
            {
                loadedSaveFileIndex = -1;
                Dictionary<string, string> nodeArguments = evt.arguments[evt.nodeName];
                if (nodeArguments.ContainsKey("index"))
                {
                    int index = int.Parse(nodeArguments["index"]);
                    selectedSaveFileIndex = index;
                    
                    SaveFile saveFileElement = (SaveFile)saveFileList.GetRootElementForIndex(selectedSaveFileIndex);
                    string saveFilePath = saveFiles[selectedSaveFileIndex];
                    SaveManager.GameSave gameSave = FileSystem.ReadDataFromFile<SaveManager.GameSave>(saveFilePath);

                    saveManagementSaveName.text = gameSave.saveName;

                    saveFileElement.AddToClassList("selected");
                }

                saveLoadButton.Focus();
            });

            hostingSelctionAnimationContainer.RegisterCallback<NavigatorOpenedEvent>((evt) =>
            {
                Dictionary<string, string> nodeArguments = evt.arguments[evt.nodeName];
                if (nodeArguments.ContainsKey("type"))
                {
                    hostingModeSelectionType = nodeArguments["type"] switch
                    {
                        "continue" => HostingModeSelectionType.ContinuedGameSave,
                        "load" => HostingModeSelectionType.LoadedGameSave,
                        "new" => HostingModeSelectionType.NewGameSave,
                        _ => HostingModeSelectionType.None
                    };
                }
            });

            navigator.Hierarchy = new NavigationNode("mainNavigation", mainNavigationContainer, 0, loadGameButton, mainNavigationContainer, true, new()
            {
                new("screenCover", screenCover, 0.5f, null, null, false, new()
                {
                    new("hostingSelection", hostingSelctionAnimationContainer, 0.5f, singleplayerModeButton),
                }),
                new("loadGame", saveMenu, 0.1f, saveFileList, saveFileList, true, new()
                {
                    new("save", saveManagement, 0.1f, saveLoadButton, saveManagement, true, new()
                    {
                        new("screenCover", screenCover, 0.5f, null, null, false, new()
                        {
                            new("hostingSelection", hostingSelctionAnimationContainer, 0.5f, singleplayerModeButton)
                        })
                    })
                }),
                new("settings", settingsMenu, 0.1f)
            });
        }



        private void Update()
        {
            Focusable currentFocus = uiDocument.rootVisualElement.focusController.focusedElement;
            if (currentFocus != lastFocus)
            {
                lastFocus = currentFocus;
                Debug.Log($"New focus: {currentFocus}");
            }
        }

        private void SelectListViewSelectedSaveFile()
        {
            if (selectedSaveFileIndex < 0 && loadedSaveFileIndex < 0 && saveFileList.selectedIndex >= 0) navigator.NavigateTo($"/loadGame/save?index={saveFileList.selectedIndex}");
        }

        private void LoadSelectedSaveFile()
        {
            Debug.Log(selectedSaveFileIndex);
            if (selectedSaveFileIndex >= 0)
            {
                loadedSaveFileIndex = selectedSaveFileIndex;
                navigator.NavigateTo($"/loadGame/save?index={loadedSaveFileIndex}/screenCover/hostingSelection?type=load");
            }
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
            Debug.Log("MULTIPLAYER CHOSEN");

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

                        StartMultiplayer();
                    }
                    break;
                case HostingModeSelectionType.NewGameSave:
                    SaveManager.Instance.CreateNewSave();
                    StartMultiplayer();
                    break;
            }
            
        }

        public async void JoinLocalHostGame()
        {
            NetworkManager.singleton.StartClient(true);
            await UniTask.WaitUntil(() => NetworkClient.active);

            Debug.Log("Client started.");
        }

        public async void StartSingleplayer()
        {
            NetworkManager.singleton.StartHost(false);

            await UniTask.WaitUntil(() => NetworkServer.active);
            Debug.Log("GANG!!! WE MADE IT!!");

            NetworkManager.singleton.ServerChangeScene(gameSceneName);
        }

        public async void StartMultiplayer()
        {
            NetworkManager.singleton.StartHost(true);

            await UniTask.WaitUntil(() => NetworkServer.active);

            NetworkManager.singleton.ServerChangeScene(gameSceneName);
        }

        private void OnCancel()
        {
            navigator.NavigateBack();
        }

        private void OnReset()
        {
            navigator.NavigateTo("/", true);
        }

        private void OnContinue()
        {
            navigator.NavigateTo("/screenCover/hostingSelection?type=continue");
        }
        private void OnLoadGame()
        {
            navigator.NavigateTo("/loadGame");
        }
        private void OnNewGame()
        {
            navigator.NavigateTo("/screenCover/hostingSelection?type=new");
        }
        private void OnSettings()
        {
            navigator.NavigateTo("/settings");
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
