using System;

using DigDig2.Debugging;
using DigDig2.Entity;
using DigDig2.Game;
using DigDig2.Input;
using DigDig2.UI.UxmlElements;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DigDig2.Player
{
	public class CharacterSelectSequencer : MonoBehaviour
	{
		[Header( "Config" )]
		[SerializeField] private float secondsReadyUntilStart = 1f;

		[Header( "Prefabs" )]
		[SerializeField] private GameObject playerControllerPrefab;

		[SerializeField] private GameObject maxPrefab;
		[SerializeField] private GameObject minisPrefab;
		[SerializeField] private InputContext characterSelectContext;

		[Header( "Scene Refs" )]
		[SerializeField] private Transform maxSpawnPoint;

		[SerializeField] private Transform minisSpawnPoint;

		[Header( "UI Refs" )]
		[SerializeField] private UIDocument uiDocument;

		private GameObject maxInstance;
		private GameObject minisInstance;

		private VisualElement fadeOut;
		private VisualElement playerContainer;

		private VisualElement playerOneBox;
		private VisualElement playerOneBoxLegend;
		private InputSymbol playerOneBoxLegendMove;
		private InputSymbol playerOneBoxLegendReady;
		private VisualElement playerOneBoxReady;

		private VisualElement playerTwoBox;
		private VisualElement playerTwoBoxLegend;
		private InputSymbol playerTwoBoxLegendMove;
		private InputSymbol playerTwoBoxLegendReady;
		private VisualElement playerTwoBoxReady;

		private Button startButton;

		private PlayerController playerOne;
		private int playerOneInputPlayerIndex = -1;
		private bool playerOneReady;
		private int playerOneNavigation = 2;

		private PlayerController playerTwo;
		private int playerTwoInputPlayerIndex = -1;
		private bool playerTwoReady;
		private int playerTwoNavigation = 2;

		private bool gameStarted;
		private float readyWaitTimer;

		[NonSerialized] public UnityEvent gameStartedEvent = new( );

		#region UnityCallbacks

		private void Awake( )
		{
			if ( !GameManager.Instance.IsMultiplayer )
			{
				BetterDebug.Log( "Not in multiplayer mode, disabling character select sequencer.", LogSeverity.Info );
				gameObject.SetActive( false );
				Destroy( gameObject );
			}
		}

		private void Start( )
		{
			maxInstance = Instantiate( maxPrefab );
			minisInstance = Instantiate( minisPrefab );

			maxInstance.GetComponent<EntityCharacterController>( )
				.Teleport( maxSpawnPoint.position, maxSpawnPoint.rotation.eulerAngles.y );

			minisInstance.GetComponent<EntityCharacterController>( )
				.Teleport( minisSpawnPoint.position, minisSpawnPoint.rotation.eulerAngles.y );

			InputManager.Instance.CurrentInputContext = characterSelectContext;

			fadeOut = uiDocument.rootVisualElement.Query<VisualElement>( "fadeOut" );
			playerContainer = fadeOut.Query<VisualElement>( "playerContainer" );

			playerOneBox = playerContainer.Query<VisualElement>( "playerOne" );
			playerOneBoxLegend = playerOneBox.Query<VisualElement>( "legend" );
			playerOneBoxLegendMove = playerOneBoxLegend.Query<InputSymbol>( "moveSymbol" );
			playerOneBoxLegendReady = playerOneBoxLegend.Query<InputSymbol>( "readySymbol" );
			playerOneBoxReady = playerOneBox.Query<VisualElement>( "ready" );

			playerTwoBox = playerContainer.Query<VisualElement>( "playerTwo" );
			playerTwoBoxLegend = playerTwoBox.Query<VisualElement>( "legend" );
			playerTwoBoxLegendMove = playerTwoBoxLegend.Query<InputSymbol>( "moveSymbol" );
			playerTwoBoxLegendReady = playerTwoBoxLegend.Query<InputSymbol>( "readySymbol" );
			playerTwoBoxReady = playerTwoBox.Query<VisualElement>( "ready" );

			playerOneBox.visible = false;
			playerTwoBox.visible = false;

			if ( GameManager.Instance.maxPlayerID == -1 || GameManager.Instance.minisPlayerID == -1 ) return;

			if ( GameManager.Instance.maxPlayerID == 0 )
			{
				playerOneInputPlayerIndex = GameManager.Instance.maxInputPlayerIndex;
				playerTwoInputPlayerIndex = GameManager.Instance.minisInputPlayerIndex;
				playerOneNavigation = 1;
				playerTwoNavigation = -1;
			}
			else
			{
				playerOneInputPlayerIndex = GameManager.Instance.minisInputPlayerIndex;
				playerTwoInputPlayerIndex = GameManager.Instance.maxInputPlayerIndex;
				playerOneNavigation = -1;
				playerTwoNavigation = 1;
			}

			StartGame( );
		}

		private void Update( )
		{
			if ( !gameStarted && playerOneReady && playerTwoReady )
			{
				readyWaitTimer += Time.deltaTime;

				if ( readyWaitTimer >= secondsReadyUntilStart ) StartGame( );
			}
			else
				readyWaitTimer = 0;
		}

		#endregion

		#region Input Message Calls

		private void OnInputCharacterSelectConfirm( InputInfo info )
		{
			if ( !info.context.started ) return;

			// Check for ready
			if ( !playerOneReady && info.inputPlayerIndex == playerOneInputPlayerIndex )
				playerOneReady = playerOneNavigation != 0;
			else if ( !playerTwoReady && info.inputPlayerIndex == playerTwoInputPlayerIndex ) playerTwoReady = playerTwoNavigation != 0;

			// Check if new InputPlayer
			if ( playerOneInputPlayerIndex == -1 )
			{
				playerOneInputPlayerIndex = info.inputPlayerIndex;
				playerOneBox.visible = true;
				playerOneBoxLegendMove.InputPlayerIndex = playerOneInputPlayerIndex;
				playerOneBoxLegendReady.InputPlayerIndex = playerOneInputPlayerIndex;
				playerOneNavigation = 0;
			}
			else if ( playerTwoInputPlayerIndex == -1 && playerOneInputPlayerIndex != info.inputPlayerIndex )
			{
				playerTwoInputPlayerIndex = info.inputPlayerIndex;
				playerTwoBox.visible = true;
				playerTwoBoxLegendMove.InputPlayerIndex = playerTwoInputPlayerIndex;
				playerTwoBoxLegendReady.InputPlayerIndex = playerTwoInputPlayerIndex;
				playerTwoNavigation = 0;
			}
			else if ( info.inputPlayerIndex != playerTwoInputPlayerIndex && info.inputPlayerIndex != playerOneInputPlayerIndex )
			{
				BetterDebug.Log( $"More than 2 InputPlayers trying to connect. InputPlayerIndex: [{info.inputPlayerIndex}]", LogSeverity.Warning );
				return;
			}

			UpdateInterfaceReadyStates( );
			UpdateInterfacePlayerPositions( );
		}

		private void OnInputCharacterSelectCancel( InputInfo info )
		{
			if ( !info.context.started ) return;

			if ( info.inputPlayerIndex == playerOneInputPlayerIndex )
			{
				if ( playerOneReady )
				{
					// Unready
					playerOneReady = false;
				}
			}
			else if ( info.inputPlayerIndex == playerTwoInputPlayerIndex )
			{
				if ( playerTwoReady )
				{
					// Unready
					playerTwoReady = false;
				}
			}

			UpdateInterfaceReadyStates( );
			UpdateInterfacePlayerPositions( );
		}

		private void OnInputCharacterSelectMove( InputInfo info )
		{
			if ( !info.context.started ) return;
			
			if ( info.inputPlayerIndex == playerOneInputPlayerIndex && !playerOneReady )
			{
				float input = info.context.ReadValue<float>( );
				playerOneNavigation = Math.Clamp(
					playerOneNavigation + Math.Sign( input ),
					playerTwoNavigation == -1 ? 0 : -1,
					playerTwoNavigation == 1 ? 0 : 1 
				);
			}
			else if ( info.inputPlayerIndex == playerTwoInputPlayerIndex && !playerTwoReady )
			{
				float input = info.context.ReadValue<float>( );
				playerTwoNavigation = Math.Clamp(
					playerTwoNavigation + Math.Sign( input ),
					playerOneNavigation == -1 ? 0 : -1,
					playerOneNavigation == 1 ? 0 : 1
				);
			}

			UpdateInterfacePlayerPositions( );
		}

		#endregion
		
		#region Game Start
		
		private void StartGame( )
		{
			gameStarted = true;

			bool playerOneIsMax = playerOneNavigation == 1;

			GameObject playerOneCharacterInstance = playerOneIsMax ? maxInstance : minisInstance;
			GameObject playerTwoCharacterInstance = playerOneIsMax ? minisInstance : maxInstance;

			PlayerController playerOneController = Instantiate( playerControllerPrefab ).GetComponent<PlayerController>( );
			PlayerController playerTwoController = Instantiate( playerControllerPrefab ).GetComponent<PlayerController>( );

			// player One
			playerOneCharacterInstance.transform.SetParent( playerOneController.transform );
			playerOneController.SetCharacterObject( playerOneCharacterInstance );
			playerOneController.characterType = playerOneIsMax ? CharacterType.Max : CharacterType.Minis;
			playerOneController.SetCharacterPrefab( playerOneIsMax ? maxPrefab : minisPrefab );
			playerOneController.name = $"PlayerOneController {playerOneController.characterType}";
			playerOneController.SetInputPlayerIDRecursive( playerOneInputPlayerIndex );

			// Player Two
			playerTwoCharacterInstance.transform.SetParent( playerTwoController.transform );
			playerTwoController.SetCharacterObject( playerTwoCharacterInstance );
			playerTwoController.characterType = playerOneIsMax ? CharacterType.Minis : CharacterType.Max;
			playerTwoController.SetCharacterPrefab( playerOneIsMax ? minisPrefab : maxPrefab );
			playerTwoController.name = $"PlayerTwoController {playerTwoController.characterType}";
			playerTwoController.SetInputPlayerIDRecursive( playerTwoInputPlayerIndex );

			// finalize
			GameManager.Instance.RegisterMultiplayerPlayers(
				playerOneController,
				playerOneInputPlayerIndex,
				playerTwoController,
				playerTwoInputPlayerIndex
			);

			playerOneController.entityController.Frozen = false;
			playerTwoController.entityController.Frozen = false;

			fadeOut.style.opacity = new( 0f );
			Destroy( gameObject, 0.5f );
			gameStartedEvent.Invoke( );
		}
		
		#endregion

		#region Interface
		
		private void UpdateInterfaceReadyStates( )
		{
			playerOneBoxLegend.style.display = new( playerOneReady ? DisplayStyle.None : DisplayStyle.Flex );
			playerOneBoxReady.style.display = new( playerOneReady ? DisplayStyle.Flex : DisplayStyle.None );

			playerTwoBoxLegend.style.display = new( playerTwoReady ? DisplayStyle.None : DisplayStyle.Flex );
			playerTwoBoxReady.style.display = new( playerTwoReady ? DisplayStyle.Flex : DisplayStyle.None );
		}

		private void UpdateInterfacePlayerPositions( )
		{
			playerOneBox.style.translate = new(
				new Translate(
					new( playerOneNavigation * playerContainer.resolvedStyle.width * 0.5f, LengthUnit.Pixel ),
					new( playerOneNavigation == playerTwoNavigation ? playerContainer.resolvedStyle.height * 0.5f : 0, LengthUnit.Pixel )
				)
			);

			playerTwoBox.style.translate = new(
				new Translate(
					new( playerTwoNavigation * playerContainer.resolvedStyle.width * 0.5f, LengthUnit.Pixel ),
					new( playerTwoNavigation == playerOneNavigation ? -( playerContainer.resolvedStyle.height * 0.5f ) : 0, LengthUnit.Pixel )
				)
			);
		}
		
		#endregion
	}
}
