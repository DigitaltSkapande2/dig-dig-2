using System;

using DigDig2.Debugging;

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using DigDig2.Game;
using DigDig2.Input;
using DigDig2.Player;
using DigDig2.Entity;
using DigDig2.UI.UxmlElements;



namespace DigDig2.Player
{
    public class CharacterSelectSequencer : MonoBehaviour
    {
        #region Variables

        [Header("Config")]
        [FormerlySerializedAs("secondsToHoldToDissconnect")] [SerializeField] private float secondsToHoldToDisconnect = 1.5f;
        [SerializeField] private float secondsReadyUntilStart = 0.8f;

        [Header("Prefabs")]
        [SerializeField] private GameObject playerControllerPrefab;
        [SerializeField] private GameObject maxPrefab;
        [SerializeField] private GameObject minisPrefab;
        [SerializeField] private InputContext characterSelectContext;
        [Header("Scene Refs")]
        [SerializeField] private Transform maxSpawnPoint;
        [SerializeField] private Transform minisSpawnPoint;
        [Header("UI Refs")]
        [SerializeField] private UIDocument uiDocument;
        [Header("Debug")]
        [SerializeField] bool verboseLogging = false;
        
        private GameObject maxInstance;
        private GameObject minisInstance;
        
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
        private bool playerOneReady = false;
        private int playerOneNavigation = 2;

        private PlayerController playerTwo;
        private int playerTwoInputPlayerIndex = -1;
        private bool playerTwoReady = false;
        private int playerTwoNavigation = 2;
        
        [NonSerialized] public UnityEvent gameStartedEvent = new();
        
        #endregion

        #region UnityCallbacks

        private void Awake()
        {
            if (!GameManager.Instance.IsMultiplayer)
            {
                VerboseLog("Not in multiplayer mode, disabling character select sequencer.");
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            maxInstance = Instantiate(maxPrefab);
            minisInstance = Instantiate(minisPrefab);

            maxInstance.GetComponent<EntityCharacterController>()
                .Teleport(maxSpawnPoint.position, maxSpawnPoint.rotation.eulerAngles.y);
            minisInstance.GetComponent<EntityCharacterController>()
                .Teleport(minisSpawnPoint.position, minisSpawnPoint.rotation.eulerAngles.y);

            InputManager.Instance.CurrentInputContext = characterSelectContext;
            
            playerContainer = uiDocument.rootVisualElement.Query<VisualElement>("playerContainer");

            playerOneBox = playerContainer.Query<VisualElement>("playerOne");
            playerOneBoxLegend = playerOneBox.Query<VisualElement>("legend");
            playerOneBoxLegendMove = playerOneBoxLegend.Query<InputSymbol>("move");
            playerOneBoxLegendReady = playerOneBoxLegend.Query<InputSymbol>("ready");
            playerOneBoxReady = playerOneBox.Query<VisualElement>("ready");
            
            playerTwoBox = playerContainer.Query<VisualElement>("playerTwo");
            playerTwoBoxLegend = playerTwoBox.Query<VisualElement>("legend");
            playerTwoBoxLegendMove = playerTwoBoxLegend.Query<InputSymbol>("move");
            playerTwoBoxLegendReady = playerTwoBoxLegend.Query<InputSymbol>("ready");
            playerTwoBoxReady = playerTwoBox.Query<VisualElement>("ready");

            playerOneBox.visible = false;
            playerTwoBox.visible = false;

            if (GameManager.Instance.maxPlayerID == -1 || GameManager.Instance.minisPlayerID == -1)
            {
                return;
            }

            if (GameManager.Instance.maxPlayerID == 0)
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

            StartGame();
        }


        #endregion

        #region InputMessageCalls

        private void OnInputCharacterSelectConfirm( InputInfo info )
        {
            if (!info.context.started) return;
            BetterDebug.Log(info.inputPlayerIndex);
            // Check for ready
            if (!playerOneReady && info.inputPlayerIndex == playerOneInputPlayerIndex)
            {
                playerOneReady = playerOneNavigation != 0;
            }
            else if (!playerTwoReady && info.inputPlayerIndex == playerTwoInputPlayerIndex)
            {
                playerTwoReady = playerTwoNavigation != 0;
            }

            if (playerOneReady && playerTwoReady)
            {
                Invoke(nameof(TryStartGame), secondsReadyUntilStart);
            }

            BetterDebug.Log($"Player One is: \"{playerOneInputPlayerIndex}\", Player Two is: \"{playerTwoInputPlayerIndex}\"");
            
            // Check if new InputPlayer
            if ( playerOneInputPlayerIndex == -1)
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
                OnBothPlayersJoined( );
            }
            else if (info.inputPlayerIndex != playerTwoInputPlayerIndex && info.inputPlayerIndex != playerOneInputPlayerIndex)
            {
                BetterDebug.Log($"More than 2 InputPlayers trying to connect. InputPlayerIndex: [{info.inputPlayerIndex}]", LogSeverity.Warning);
                return;
            }
            
            UpdateReadyIcons();
            UpdatePlayerIconPositions();
        }

        private void OnBothPlayersJoined()
        {
            
        }

        private void OnInputCharacterSelectCancel(InputInfo info)
        {
            if (!info.context.started) return;
            
            if (info.inputPlayerIndex == playerOneInputPlayerIndex)
            {
                if (playerOneReady)
                {
                    // Unready
                    playerOneReady = false;
                }
                else if (info.context.duration >= secondsToHoldToDisconnect)
                {
                    // disconnect player one TODO: (make player two player one)

                }
            }
            else if (info.inputPlayerIndex == playerTwoInputPlayerIndex)
            {
                if (playerTwoReady) 
                {
                    // Unready
                    playerTwoReady = false;
                }
                else
                {
                    // disconnect player two
                }
            }
            
            UpdateReadyIcons();
            UpdatePlayerIconPositions();
        }

        private void OnInputCharacterSelectMove( InputInfo info )
        {
            if (!info.context.started) return;
            print(info.context.ReadValue<Vector2>());
            if (info.inputPlayerIndex == playerOneInputPlayerIndex && !playerOneReady)
            {
                Vector2 input = info.context.ReadValue<Vector2>();
                playerOneNavigation += Math.Sign(input.x);
                playerOneNavigation = Math.Clamp(playerOneNavigation, -1, 1);
            }
            else if (info.inputPlayerIndex == playerTwoInputPlayerIndex && !playerTwoReady)
            {
                Vector2 input = info.context.ReadValue<Vector2>();
                playerTwoNavigation += Math.Sign(input.x);
                playerTwoNavigation = Math.Clamp(playerTwoNavigation, -1, 1);
            }
            
            UpdatePlayerIconPositions();
        }

        #endregion

        private void TryStartGame()
        {
            if (playerOneReady && playerTwoReady) StartGame();
        }
        
        private void StartGame()
        {
            bool playerOneIsMax = playerOneNavigation == 1;
            
            GameObject playerOneCharacterInstance = playerOneIsMax ? maxInstance : minisInstance;
            GameObject playerTwoCharacterInstance = playerOneIsMax ? minisInstance : maxInstance;

            PlayerController playerOneController = Instantiate(playerControllerPrefab).GetComponent<PlayerController>();
            PlayerController playerTwoController = Instantiate(playerControllerPrefab).GetComponent<PlayerController>();
            
            // player One
            playerOneCharacterInstance.transform.SetParent(playerOneController.transform);
            playerOneController.SetCharacterObject(playerOneCharacterInstance);
            playerOneController.characterType = playerOneIsMax ? CharacterType.Max : CharacterType.Minis;
            playerOneController.SetCharacterPrefab(playerOneIsMax ? maxPrefab : minisPrefab);
            playerOneController.name = $"PlayerOneController {playerOneController.characterType}";
            playerOneController.SetInputPlayerIDRecursive(playerOneInputPlayerIndex);
            
            // Player Two
            playerTwoCharacterInstance.transform.SetParent(playerTwoController.transform);
            playerTwoController.SetCharacterObject(playerTwoCharacterInstance);
            playerTwoController.characterType = playerOneIsMax ? CharacterType.Minis : CharacterType.Max;
            playerTwoController.SetCharacterPrefab(playerOneIsMax ? minisPrefab : maxPrefab);
            playerTwoController.name = $"PlayerTwoController {playerTwoController.characterType}";
            playerTwoController.SetInputPlayerIDRecursive(playerTwoInputPlayerIndex);
            
            // finalize
            GameManager.Instance.RegisterMultiplayerPlayers(
                playerOneController, 
                playerOneInputPlayerIndex,
                playerTwoController,
                playerTwoInputPlayerIndex
            );
            
            playerOneController.entityController.Frozen = false;
            playerTwoController.entityController.Frozen = false;

            uiDocument.rootVisualElement.style.opacity = new StyleFloat(0f);
            Invoke(nameof(Die), 0.5f);
            gameStartedEvent?.Invoke();
        }

        private void Die()
        {
            Destroy(gameObject);
        }

        private bool CanStartGame()
        {
            return playerOneNavigation != playerTwoNavigation && playerOneNavigation != 0 && playerTwoNavigation != 0 &&
                   playerTwo.inputPlayerIndex != -1 && playerTwo.inputPlayerIndex != -1;
        }
        

        private void UpdateReadyIcons()
        {
            playerOneBoxLegend.style.display = new StyleEnum<DisplayStyle>(playerOneReady ? DisplayStyle.None : DisplayStyle.Flex);
            playerOneBoxReady.style.display = new StyleEnum<DisplayStyle>(playerOneReady ? DisplayStyle.Flex : DisplayStyle.None);
            
            playerTwoBoxLegend.style.display = new StyleEnum<DisplayStyle>(playerTwoReady ? DisplayStyle.None : DisplayStyle.Flex);
            playerTwoBoxReady.style.display = new StyleEnum<DisplayStyle>(playerTwoReady ? DisplayStyle.Flex : DisplayStyle.None);
        }

        private void UpdatePlayerIconPositions()
        {
            playerOneBox.style.translate = new StyleTranslate(
                new Translate(
                    new Length(playerOneNavigation * playerContainer.resolvedStyle.width * 0.5f, LengthUnit.Pixel),
                    new Length(playerOneNavigation == playerTwoNavigation ? playerContainer.resolvedStyle.height * 0.5f : 0, LengthUnit.Pixel)
                )
            );
            
            
            playerTwoBox.style.translate = new StyleTranslate(
                new Translate(
                    new Length(playerTwoNavigation * playerContainer.resolvedStyle.width * 0.5f, LengthUnit.Pixel),
                    new Length(playerTwoNavigation == playerOneNavigation ? -(playerContainer.resolvedStyle.height * 0.5f) : 0, LengthUnit.Pixel)
                )
            );
        }
        
        #region Util

        private void VerboseLog(string msg)
        {
            if (verboseLogging) Debug.Log("CHARACTER_SELECT: " + msg);
        }
        
        #endregion
    }
}