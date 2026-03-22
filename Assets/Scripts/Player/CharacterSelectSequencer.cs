
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


namespace DigDig2
{
    public class CharacterSelectSequencer : MonoBehaviour
    {
        #region Variables

        [Header("config")]
        [SerializeField] private float secondsToHoldToDissconnect = 1.5f;
        [SerializeField] private float secondsReadyUntilStart = 0.8f;
        [Header("Prefabs")]
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
        private Image playerOneCharacterImage;
        private VisualElement playerOneReadyIcon;
        private Image playerTwoCharacterImage;
        private VisualElement playerTwoReadyIcon;
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
            VisualElement visualElementBoundBox = realRoot.Query("BoundBox");

            playerOneCharacterImage = visualElementBoundBox.Query<Image>("PlayerOne");
            playerOneReadyIcon = playerOneCharacterImage.Query("ReadyIcon");
            playerTwoCharacterImage = visualElementBoundBox.Query<Image>("PlayerTwo");
            playerTwoReadyIcon = playerTwoCharacterImage.Query("ReadyIcon");
            
            playerOneCharacterImage.visible = false;
            playerTwoCharacterImage.visible = false;
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

            UpdateReadyIcons();
            
            // Check if new InputPlayer
            if ( playerOneInputPlayerindex == -1)
            {
                playerOneInputPlayerindex = info.inputPlayerIndex;
                playerOneCharacterImage.visible = true;
                BetterDebug.Log( $"Player One is '{info.inputPlayerIndex}'" );
                playerOneNavigation = 0;
            }
            else if ( playerTwoInputPlayerindex == -1 && playerOneInputPlayerindex != info.inputPlayerIndex )
            {
                playerTwoInputPlayerindex = info.inputPlayerIndex;
                playerTwoCharacterImage.visible = true;
                playerTwoNavigation = 0;
                BetterDebug.Log( $"Player Two is '{info.inputPlayerIndex}'" );
                OnBothPlayersJoined( );
            }
            else if (info.inputPlayerIndex != playerTwoInputPlayerindex && info.inputPlayerIndex != playerOneInputPlayerindex)
            {
                BetterDebug.Log($"More than 2 InputPlayers trying to connect. InputPlayerIndex: [{info.inputPlayerIndex}]", LogSeverity.Warning);
                return;
            }
            
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
                    playerOne = null;
                    playerOneNavigation = -1;
                    playerOneCharacterImage.visible = false;

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
                    playerTwo = null;
                    playerTwoNavigation = -1;
                    playerTwoCharacterImage.visible = false;
                }
            }
            
            UpdateReadyIcons();
            UpdatePlayerIconPositions();
        }

        private void OnInputCharacterSelectMove( InputInfo info )
        {
            if (!info.context.started) return;
            print(info.context.ReadValue<Vector2>());
            if (info.inputPlayerIndex == playerOneInputPlayerindex)
            {
                Vector2 input = info.context.ReadValue<Vector2>();
                playerOneNavigation += Math.Sign(input.x);
                playerOneNavigation = Math.Clamp(playerOneNavigation, -1, 1);
            }
            else if (info.inputPlayerIndex == playerTwoInputPlayerindex)
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

            PlayerController playerOneController = playerOneCharacterInstance.GetComponent<PlayerController>();
            PlayerController playerTwoController = playerTwoCharacterInstance.GetComponent<PlayerController>();
            
            playerOneController.characterType = playerOneIsMax ? CharacterType.Max : CharacterType.Minis;
            playerOneController.SetInputPlayerID(playerOneInputPlayerindex);
            
            playerTwoController.characterType = playerOneIsMax ? CharacterType.Minis : CharacterType.Max;
            playerTwoController.SetInputPlayerID(playerTwoInputPlayerindex);
            
            GameManager.Instance.RegisterMultiplayerPlayers(
                playerOneController, 
                playerTwoController 
            );
            
            playerOneCharacterInstance.GetComponent<EntityCharacterController>().Frozen = false;
            playerOneCharacterInstance.GetComponent<EntityCharacterController>().Frozen = false;

            realRoot.style.opacity = new StyleFloat(0f);
            Invoke(nameof(Die), 0.5f);
            gameStartedEvent.Invoke();
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
            playerOneReadyIcon.style.opacity = new StyleFloat(playerOneReady ? 100 : 0);
            playerOneReadyIcon.style.scale =
                new StyleScale(playerOneReady ? new Vector2(1.5f, 1.5f) : new Vector2(1f, 1f));

            playerTwoReadyIcon.style.opacity = new StyleFloat(playerTwoReady ? 100 : 0);
            playerTwoReadyIcon.style.scale =
                new StyleScale(playerTwoReady ? new Vector2(1.5f, 1.5f) : new Vector2(1f, 1f));
        }

        private void UpdatePlayerIconPositions()
        {
            playerOneCharacterImage.style.translate = new StyleTranslate(
                new Translate(
                    new Length(playerOneNavigation * 50, LengthUnit.Percent),
                    new Length(playerOneNavigation == playerTwoNavigation ? 50 : 0, LengthUnit.Percent)
                )
            );
            
            
            playerTwoCharacterImage.style.translate = new StyleTranslate(
                new Translate(
                    new Length(playerTwoNavigation * 50, LengthUnit.Percent),
                    new Length(playerTwoNavigation == playerOneNavigation ? -50 : 0, LengthUnit.Percent)
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