
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using DigDig2.Game;
using DigDig2.Player;


namespace DigDig2
{
    public class CharacterSelectSequencer : MonoBehaviour
    {
        #region Variables
        [Header("Prefabs")]
        [SerializeField] private GameObject maxPrefab;
        [SerializeField] private GameObject minisPrefab;
        [SerializeField] private GameObject playerInputManagerPrefab; 
        [Header("Scene Refs")]
        [SerializeField] private Transform maxSpawnPoint;
        [SerializeField] private Transform minisSpawnPoint;
        [Header("UI Refs")]
        [SerializeField] private UIDocument uiDocument;
        [Header("Debug")]
        [SerializeField] bool verboseLogging = false;
        [FormerlySerializedAs("hostIsMax")] [SerializeField] private bool PlayerOneIsMax = true;
        
        private GameObject maxDummyInstance;
        private GameObject minisDummyInstance;
        
        private PlayerInputManager playerInputManager;
        
        private PlayerInput playerOneInput;
        private PlayerInput playerTwoInput;

        private VisualElement realRoot; // For Opacity Transition
        private Image playerOneCharacterImage;
        private Image playerTwoCharacterImage;
        private Button startButton;

        private int playerOneNavigation = 2;
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
            
            playerInputManager = Instantiate(playerInputManagerPrefab).GetComponent<PlayerInputManager>();
            playerInputManager.playerJoinedEvent.AddListener(OnPlayerJoined);
        }

        private void Start()
        {
            maxDummyInstance = Instantiate(maxPrefab, maxSpawnPoint.position, maxSpawnPoint.rotation);
            minisDummyInstance = Instantiate(minisPrefab, minisSpawnPoint.position, minisSpawnPoint.rotation);

            realRoot = uiDocument.rootVisualElement.Query("RealRoot");
            VisualElement visualElementBoundBox = realRoot.Query("BoundBox");
            startButton = realRoot.Query<Button>("StartButton");
            startButton.visible = false;
            startButton.clicked += OnStartButtonClicked;

            playerOneCharacterImage = visualElementBoundBox.Query<Image>("PlayerOne");
            playerTwoCharacterImage = visualElementBoundBox.Query<Image>("PlayerTwo");
            
            playerOneCharacterImage.visible = false;
            playerTwoCharacterImage.visible = false;
        }

        private void OnEnable()
        {
            playerInputManager.EnableJoining();
        }

        #endregion
        
        private void OnSwitchCharacterButtonClicked()
        {
            PlayerOneIsMax = !PlayerOneIsMax;
        }
        
        private async void OnStartButtonClicked()
        {
            Destroy(maxDummyInstance);
            Destroy(minisDummyInstance);

            GameObject maxInstance = Instantiate(maxPrefab, maxSpawnPoint.position, maxSpawnPoint.rotation);
            GameObject minisInstance = Instantiate(minisPrefab, minisSpawnPoint.position, minisSpawnPoint.rotation);

            GameObject playerOneCharacterInstance = PlayerOneIsMax ? maxInstance : minisInstance;
            GameObject playerTwoCharacterInstance = PlayerOneIsMax ? minisInstance : maxInstance;
            
            playerOneCharacterInstance.transform.SetParent(playerOneInput.transform);
            playerTwoCharacterInstance.transform.SetParent(playerTwoInput.transform);
            
            GameManager.Instance.RegisterMultiplayerPlayers(playerOneCharacterInstance, playerTwoCharacterInstance);
            
            playerOneCharacterInstance.GetComponent<EntityCharacterController>().Frozen = false;
            playerOneCharacterInstance.GetComponent<EntityCharacterController>().Frozen = false;
            playerOneInput.notificationBehavior = PlayerNotifications.BroadcastMessages;
            playerTwoInput.notificationBehavior = PlayerNotifications.BroadcastMessages;

            realRoot.style.opacity = new StyleFloat(0f);
            Invoke(nameof(DisableUIDoc), 0.5f);
            gameStartedEvent.Invoke();
        }

        private void DisableUIDoc()
        {
            uiDocument.enabled = false;
        }

        private void EnablePlayerInput() 
        {

        }

        public void OnPlayerJoined(PlayerInput playerInput)
        {
            if (playerOneInput == null)
            {
                playerOneInput = playerInput;
                playerInput.onActionTriggered += OnPlayerOneInputActionTriggered;
                playerOneNavigation = 0;
                playerOneCharacterImage.visible = true;
            }
            else if (playerTwoInput == null)
            {
                playerTwoInput = playerInput;
                playerTwoCharacterImage.visible = true;
                playerInput.onActionTriggered += OnPlayerTwoInputActionTriggered;
                playerTwoNavigation = 0;
                OnBothPlayersJoined();
            }
            else
            {
                Debug.LogError("Too many players joined, this should not be allowed by the PlayerInputManager.");
            }
            
            playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

            UpdatePlayerIconPositions();
            
            print($"Player joined, {playerInput.user}");
        }

        private void OnPlayerOneInputActionTriggered(InputAction.CallbackContext context)
        {
            if (context.started && context.action.name == "Move")
            {
                Vector2 input = context.ReadValue<Vector2>();

                playerOneNavigation += Math.Sign(input.x);
                playerOneNavigation = Math.Clamp(playerOneNavigation, -1, 1);
                UpdatePlayerIconPositions();
            }
        }
        
        private void OnPlayerTwoInputActionTriggered(InputAction.CallbackContext context)
        {
            if (context.started && context.action.name == "Move")
            {
                Vector2 input = context.ReadValue<Vector2>();
                playerTwoNavigation += Math.Sign(input.x);
                playerTwoNavigation = Math.Clamp(playerTwoNavigation, -1, 1);
                
                UpdatePlayerIconPositions();
            }
        }

        private bool CanStartGame()
        {
            return playerOneNavigation != playerTwoNavigation && playerOneNavigation != 0 && playerTwoNavigation != 0 &&
                   playerOneInput && playerTwoInput;
        }

        private void OnBothPlayersJoined()
        {
            
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

            if (CanStartGame())
            {
                if ( playerOneInput.devices[0] is Keyboard || playerOneInput.devices[0] is Mouse )
                {
                    startButton.visible = true;
                }
                else if ( playerOneInput.devices[0] is Gamepad )
                {
                    
                }
            }
            else
            {
                startButton.visible = false;
            }
        }
        
        #region Util

        private void VerboseLog(string msg)
        {
            if (verboseLogging) Debug.Log("CHARACTER_SELECT: " + msg);
        }
        
        #endregion
    }
}