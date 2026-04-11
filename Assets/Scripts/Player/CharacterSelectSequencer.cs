
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


namespace DigDig2
{
    public class CharacterSelectSequencer : MonoBehaviour
    {
        #region Variables

        [Header("config")]
        [SerializeField] private float secondsToHoldToDissconnect = 1.5f;
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

        private VisualElement realRoot; // For Opacity Transition
        private VisualElement playerBoundingBox;
        private VisualElement playerOneCharacterImage;
        private VisualElement playerOneReadyIcon;
        private InputSymbol playerOneReadyInputSymbol;
        private VisualElement playerTwoCharacterImage;
        private VisualElement playerTwoReadyIcon;
        private InputSymbol playerTwoReadyInputSymbol;
        private Button startButton;

        private PlayerController playerOne;
        private int playerOneInputPlayerindex = -1;
        private bool playerOneReady = false;
        private int playerOneNavigation = 2;

        private PlayerController playerTwo;
        private int playerTwoInputPlayerindex = -1;
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

            realRoot = uiDocument.rootVisualElement.Query("RealRoot");
            playerBoundingBox = realRoot.Query("BoundBox");

            playerOneCharacterImage = playerBoundingBox.Query("PlayerOne");
            VisualElement playerOneCheckBox = playerOneCharacterImage.Query("checkBox");
            playerOneReadyInputSymbol = playerOneCheckBox.Query<InputSymbol>("readyInputSymbol");
            playerOneReadyIcon = playerOneCheckBox.Query("readyCheckMark");
            playerTwoCharacterImage = playerBoundingBox.Query("PlayerTwo");
            VisualElement playerTwoCheckBox = playerTwoCharacterImage.Query("checkBox");
            playerTwoReadyInputSymbol = playerTwoCheckBox.Query<InputSymbol>("readyInputSymbol");
            playerTwoReadyIcon = playerTwoCheckBox.Query("readyCheckMark");

            playerOneCharacterImage.visible = false;
            playerTwoCharacterImage.visible = false;

            if (GameManager.Instance.maxPlayerID == -1 || GameManager.Instance.minisPlayerID == -1)
            {
                return;
            }

            if (GameManager.Instance.maxPlayerID == 0)
            {
                playerOneInputPlayerindex = GameManager.Instance.maxInputPlayerIndex;
                playerTwoInputPlayerindex = GameManager.Instance.minisInputPlayerIndex;
                playerOneNavigation = 1;
                playerTwoNavigation = -1;
            }
            else
            {
                playerOneInputPlayerindex = GameManager.Instance.minisInputPlayerIndex;
                playerTwoInputPlayerindex = GameManager.Instance.maxInputPlayerIndex;
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
            if (!playerOneReady && info.inputPlayerIndex == playerOneInputPlayerindex)
            {
                playerOneReady = playerOneNavigation != 0;
            }
            else if (!playerTwoReady && info.inputPlayerIndex == playerTwoInputPlayerindex)
            {
                playerTwoReady = playerTwoNavigation != 0;
            }

            if (playerOneReady && playerTwoReady)
            {
                Invoke(nameof(TryStartGame), secondsReadyUntilStart);
            }

            
            
            // Check if new InputPlayer
            if ( playerOneInputPlayerindex == -1)
            {
                playerOneInputPlayerindex = info.inputPlayerIndex;
                playerOneCharacterImage.visible = true;
                playerOneReadyInputSymbol.InputPlayerIndex = playerOneInputPlayerindex;
                BetterDebug.Log( $"Player One is '{info.inputPlayerIndex}'" );
                playerOneNavigation = 0;
            }
            else if ( playerTwoInputPlayerindex == -1 && playerOneInputPlayerindex != info.inputPlayerIndex )
            {
                playerTwoInputPlayerindex = info.inputPlayerIndex;
                playerTwoCharacterImage.visible = true;
                playerTwoReadyInputSymbol.InputPlayerIndex = playerTwoInputPlayerindex;
                playerTwoNavigation = 0;
                BetterDebug.Log( $"Player Two is '{info.inputPlayerIndex}'" );
                OnBothPlayersJoined( );
            }
            else if (info.inputPlayerIndex != playerTwoInputPlayerindex && info.inputPlayerIndex != playerOneInputPlayerindex)
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
            
            if (info.inputPlayerIndex == playerOneInputPlayerindex)
            {
                if (playerOneReady)
                {
                    // Unready
                    playerOneReady = false;
                }
                else if (info.context.duration >= secondsToHoldToDissconnect)
                {
                    // disconnect player one TODO: (make player two player one)

                }
            }
            else if (info.inputPlayerIndex == playerTwoInputPlayerindex)
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
            if (info.inputPlayerIndex == playerOneInputPlayerindex && !playerOneReady)
            {
                Vector2 input = info.context.ReadValue<Vector2>();
                playerOneNavigation += Math.Sign(input.x);
                playerOneNavigation = Math.Clamp(playerOneNavigation, -1, 1);
            }
            else if (info.inputPlayerIndex == playerTwoInputPlayerindex && !playerTwoReady)
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
            playerOneController.SetInputPlayerIDRecursive(playerOneInputPlayerindex);
            
            // Player Two
            playerTwoCharacterInstance.transform.SetParent(playerTwoController.transform);
            playerTwoController.SetCharacterObject(playerTwoCharacterInstance);
            playerTwoController.characterType = playerOneIsMax ? CharacterType.Minis : CharacterType.Max;
            playerTwoController.SetCharacterPrefab(playerOneIsMax ? minisPrefab : maxPrefab);
            playerTwoController.name = $"PlayerTwoController {playerTwoController.characterType}";
            playerTwoController.SetInputPlayerIDRecursive(playerTwoInputPlayerindex);
            
            // finalize
            GameManager.Instance.RegisterMultiplayerPlayers(
                playerOneController, 
                playerOneInputPlayerindex,
                playerTwoController,
                playerTwoInputPlayerindex
            );
            
            playerOneController.entityController.Frozen = false;
            playerTwoController.entityController.Frozen = false;

            realRoot.style.opacity = new StyleFloat(0f);
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
            playerOneReadyInputSymbol.style.opacity = new StyleFloat(playerOneReady ? 0 : 100);
            playerOneReadyIcon.style.opacity = new StyleFloat(playerOneReady ? 100 : 0);
            playerOneReadyIcon.style.scale =
                new StyleScale(playerOneReady ? new Vector2(1.5f, 1.5f) : new Vector2(1f, 1f));

            playerTwoReadyInputSymbol.style.opacity = new StyleFloat(playerTwoReady ? 0 : 100);
            playerTwoReadyIcon.style.opacity = new StyleFloat(playerTwoReady ? 100 : 0);
            playerTwoReadyIcon.style.scale =
                new StyleScale(playerTwoReady ? new Vector2(1.5f, 1.5f) : new Vector2(1f, 1f));
        }

        private void UpdatePlayerIconPositions()
        {
            BetterDebug.Log(playerOneCharacterImage.name);
            playerOneCharacterImage.style.translate = new StyleTranslate(
                new Translate(
                    new Length(playerOneNavigation * playerBoundingBox.resolvedStyle.width * 0.5f, LengthUnit.Pixel),
                    new Length(playerOneNavigation == playerTwoNavigation ? playerBoundingBox.resolvedStyle.height * 0.5f : 0, LengthUnit.Pixel)
                )
            );
            
            
            playerTwoCharacterImage.style.translate = new StyleTranslate(
                new Translate(
                    new Length(playerTwoNavigation * playerBoundingBox.resolvedStyle.width * 0.5f, LengthUnit.Pixel),
                    new Length(playerTwoNavigation == playerOneNavigation ? -(playerBoundingBox.resolvedStyle.height * 0.5f) : 0, LengthUnit.Pixel)
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