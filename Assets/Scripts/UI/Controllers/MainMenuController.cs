using System;
using System.Collections.Generic;

using DigDig2.SaveSystem;
using DigDig2.UI.Events;
using DigDig2.UI.Navigation;
using DigDig2.UI.UxmlElements;

using UnityEditor;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace DigDig2.UI.Controllers {
	[RequireComponent( typeof( UIDocument ), typeof( UserInterfaceNavigator ) )]
	public class MainMenuController : MonoBehaviour {
		[SerializeField] private string gameSceneName = "GameScene";

		[SerializeField] private GameObject buttonHoverEffectPrefab;
		[SerializeField] private GameObject buttonClickEffectPrefab;
		private Button continueButton;

		private HostingModeSelectionType hostingModeSelectionType = HostingModeSelectionType.None;
		private VisualElement hostingSelectionAnimationContainer;
		private Button joinGameButton;
		private int loadedSaveFileIndex = -1;
		private Button loadGameButton;

		private VisualElement mainNavigationContainer;
		private Button multiplayerModeButton;
		private UserInterfaceNavigator navigator;
		private Button newGameButton;
		private Button quitButton;
		private Button saveDeleteButton;
		private SubmittableListView saveFileList;
		private List<string> saveFiles;
		private Button saveLoadButton;

		private VisualElement saveManagement;
		private Label saveManagementSaveName;

		private VisualElement saveMenu;
		private Button saveRenameButton;

		private VisualElement screenCover;
		private int selectedSaveFileIndex = -1;

		private Button settingsButton;

		private VisualElement settingsMenu;
		private Button singleplayerModeButton;

		private UIDocument uiDocument;

		private void Awake( ) {
			uiDocument = GetComponent<UIDocument>( );
			navigator = GetComponent<UserInterfaceNavigator>( );
		}

		private void Start( ) {
			saveFiles = SaveManager.Instance.GetSaveFiles( );

			uiDocument.rootVisualElement.RegisterCallback<ClickEvent>( ButtonClick, TrickleDown.TrickleDown );
			uiDocument.rootVisualElement.RegisterCallback<NavigationSubmitEvent>( ButtonClick, TrickleDown.TrickleDown );
			uiDocument.rootVisualElement.RegisterCallback<FocusEvent>( ButtonHover, TrickleDown.TrickleDown );

			mainNavigationContainer = uiDocument.rootVisualElement.Query<VisualElement>( "mainNavigation" );

			continueButton = mainNavigationContainer.Query<Button>( "continue" );
			continueButton.clicked += OnContinue;

			loadGameButton = mainNavigationContainer.Query<Button>( "loadGame" );
			loadGameButton.clicked += OnLoadGame;

			newGameButton = mainNavigationContainer.Query<Button>( "newGame" );
			newGameButton.clicked += OnNewGame;

			settingsButton = mainNavigationContainer.Query<Button>( "settings" );
			settingsButton.clicked += OnSettings;

			quitButton = mainNavigationContainer.Query<Button>( "quit" );
			quitButton.clicked += OnQuit;

			// Save Menu
			saveMenu = uiDocument.rootVisualElement.Query<VisualElement>( "saveMenu" );

			saveFileList = saveMenu.Query<SubmittableListView>( "saveFileList" );
			saveFileList.makeItem = ( ) => new SaveFile( );
			saveFileList.bindItem = ( element, index ) => {
				var saveFileElement = (SaveFile)element;
				string saveFilePath = saveFiles[ index ];
				SaveManager.GameSave gameSave = FileSystem.ReadDataFromFile<SaveManager.GameSave>( saveFilePath );
				saveFileElement.SaveName = gameSave.saveName;
				saveFileElement.SaveInfo = $"Version: {gameSave.version}";
			};

			saveFileList.itemsSource = saveFiles;
			saveFileList.selectionType = SelectionType.Single;
			saveFileList.Submit += SelectListViewSelectedSaveFile;

			saveManagement = saveMenu.Query<VisualElement>( "saveManagement" );

			saveManagementSaveName = saveMenu.Query<Label>( "saveName" );

			saveLoadButton = saveManagement.Query<Button>( "load" );
			saveLoadButton.clicked += LoadSelectedSaveFile;

			saveRenameButton = saveManagement.Query<Button>( "rename" );

			saveDeleteButton = saveManagement.Query<Button>( "delete" );

			// Settings Menu
			settingsMenu = uiDocument.rootVisualElement.Query<VisualElement>( "settingsMenu" );

			// Hosting Screen
			screenCover = uiDocument.rootVisualElement.Query<VisualElement>( "screenCover" );

			hostingSelectionAnimationContainer = screenCover.Query<VisualElement>( "animationContainer" );

			singleplayerModeButton = hostingSelectionAnimationContainer.Query<Button>( "singleplayer" );
			singleplayerModeButton.clicked += OnSingleplayerChosen;

			multiplayerModeButton = hostingSelectionAnimationContainer.Query<Button>( "multiplayer" );
			multiplayerModeButton.clicked += OnMultiplayerChosen;

			saveMenu.RegisterCallback<NavigatorOpenedEvent>( evt => {
					selectedSaveFileIndex = -1;
					for ( int index = 0; index < saveFiles.Count; index++ ) {
						var saveFileElement = (SaveFile)saveFileList.GetRootElementForIndex( index );
						saveFileElement?.RemoveFromClassList( "selected" );
					}
				}
			);

			saveManagement.RegisterCallback<NavigatorOpenedEvent>( evt => {
					loadedSaveFileIndex = -1;
					Dictionary<string, string> nodeArguments = evt.arguments[ evt.nodeName ];
					if ( nodeArguments.TryGetValue( "index", out string nodeArgument ) ) {
						int index = int.Parse( nodeArgument );
						selectedSaveFileIndex = index;

						var saveFileElement = (SaveFile)saveFileList.GetRootElementForIndex( selectedSaveFileIndex );
						string saveFilePath = saveFiles[ selectedSaveFileIndex ];
						SaveManager.GameSave gameSave = FileSystem.ReadDataFromFile<SaveManager.GameSave>( saveFilePath );

						saveManagementSaveName.text = gameSave.saveName;

						saveFileElement.AddToClassList( "selected" );
					}

					saveLoadButton.Focus( );
				}
			);

			hostingSelectionAnimationContainer.RegisterCallback<NavigatorOpenedEvent>( evt => {
					Dictionary<string, string> nodeArguments = evt.arguments[ evt.nodeName ];
					if ( nodeArguments.ContainsKey( "type" ) ) {
						hostingModeSelectionType = nodeArguments[ "type" ] switch {
							"continue" => HostingModeSelectionType.ContinuedGameSave,
							"load" => HostingModeSelectionType.LoadedGameSave,
							"new" => HostingModeSelectionType.NewGameSave,
							_ => HostingModeSelectionType.None
						};
					}
				}
			);

			navigator.Hierarchy = new(
				"mainNavigation",
				mainNavigationContainer,
				0,
				loadGameButton,
				mainNavigationContainer,
				true,
				new( ) {
					new(
						"screenCover",
						screenCover,
						0.5f,
						null,
						null,
						false,
						new( ) {
							new( "hostingSelection", hostingSelectionAnimationContainer, 0.5f, singleplayerModeButton )
						}
					),
					new(
						"loadGame",
						saveMenu,
						0.1f,
						saveFileList,
						saveFileList,
						true,
						new( ) {
							new(
								"save",
								saveManagement,
								0.1f,
								saveLoadButton,
								saveManagement,
								true,
								new( ) {
									new(
										"screenCover",
										screenCover,
										0.5f,
										null,
										null,
										false,
										new( ) {
											new( "hostingSelection", hostingSelectionAnimationContainer, 0.5f, singleplayerModeButton )
										}
									)
								}
							)
						}
					),
					new( "settings", settingsMenu, 0.1f )
				}
			);
		}

		private void ButtonClick( EventBase _ ) { PlaySoundEffect( buttonClickEffectPrefab ); }

		private void ButtonHover( EventBase _ ) { PlaySoundEffect( buttonHoverEffectPrefab ); }

		private void PlaySoundEffect( GameObject effectPrefab ) { Destroy( Instantiate( effectPrefab, Vector3.zero, Quaternion.identity, transform ), 10f ); }

		private void SelectListViewSelectedSaveFile( ) {
			if ( selectedSaveFileIndex < 0 && loadedSaveFileIndex < 0 && saveFileList.selectedIndex >= 0 ) navigator.NavigateTo( $"/loadGame/save?index={saveFileList.selectedIndex}" );
		}

		private void LoadSelectedSaveFile( ) {
			Debug.Log( selectedSaveFileIndex );
			if ( selectedSaveFileIndex < 0 ) return;

			loadedSaveFileIndex = selectedSaveFileIndex;
			navigator.NavigateTo( $"/loadGame/save?index={loadedSaveFileIndex}/screenCover/hostingSelection?type=load" );
		}

		private void OnSingleplayerChosen( ) {
			switch ( hostingModeSelectionType ) {
				case HostingModeSelectionType.ContinuedGameSave: Debug.LogWarning( "Continuing not implemented yet!" ); break;
				case HostingModeSelectionType.LoadedGameSave:
					if ( loadedSaveFileIndex >= 0 ) {
						string saveFilePath = saveFiles[ loadedSaveFileIndex ];
						SaveManager.GameSave gameSave = FileSystem.ReadDataFromFile<SaveManager.GameSave>( saveFilePath );
						SaveManager.Instance.LoadSave( gameSave );

						StartSingleplayer( );
					}

					break;
				case HostingModeSelectionType.NewGameSave:
					SaveManager.Instance.CreateNewSave( );
					StartSingleplayer( );
					break;
				case HostingModeSelectionType.None: break;
				default: throw new ArgumentOutOfRangeException( );
			}
		}

		private void OnMultiplayerChosen( ) {
			Debug.Log( "MULTIPLAYER CHOSEN" );

			switch ( hostingModeSelectionType ) {
				case HostingModeSelectionType.ContinuedGameSave: Debug.LogWarning( "Continuing not implemented yet!" ); break;
				case HostingModeSelectionType.LoadedGameSave:
					if ( loadedSaveFileIndex >= 0 ) {
						string saveFilePath = saveFiles[ loadedSaveFileIndex ];
						SaveManager.GameSave gameSave = FileSystem.ReadDataFromFile<SaveManager.GameSave>( saveFilePath );
						SaveManager.Instance.LoadSave( gameSave );

						StartMultiplayer( );
					}

					break;
				case HostingModeSelectionType.NewGameSave:
					SaveManager.Instance.CreateNewSave( );
					StartMultiplayer( );
					break;
				case HostingModeSelectionType.None: break;
				default: throw new ArgumentOutOfRangeException( );
			}
		}

		public void StartSingleplayer( ) { SceneManager.LoadScene( gameSceneName ); }

		public void StartMultiplayer( ) { SceneManager.LoadScene( gameSceneName ); }

		private void OnCancel( ) { navigator.NavigateBack( ); }

		private void OnReset( ) { navigator.NavigateTo( "/", true ); }

		private void OnContinue( ) { navigator.NavigateTo( "/screenCover/hostingSelection?type=continue" ); }

		private void OnLoadGame( ) { navigator.NavigateTo( "/loadGame" ); }

		private void OnNewGame( ) { navigator.NavigateTo( "/screenCover/hostingSelection?type=new" ); }

		private void OnSettings( ) { navigator.NavigateTo( "/settings" ); }

		private void OnQuit( ) {
			#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
			#endif
			#if UNITY_STANDALONE
			Application.Quit( );
			#endif
		}

		private enum HostingModeSelectionType {
			None,
			ContinuedGameSave,
			LoadedGameSave,
			NewGameSave
		}
	}
}
