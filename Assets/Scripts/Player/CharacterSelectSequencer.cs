
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using DigDig2.Game;


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
        
        PlayerInputManager playerInputManager;
        
        PlayerInput playerOneInput;
        PlayerInput playerTwoInput;
        
        Image playerOneCharacterImage;
        Image playerTwoCharacterImage;
        
        [NonSerialized] public UnityEvent gameStartedEvent;
        
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

            VisualElement visualElementBoundBox = uiDocument.rootVisualElement.Query("BoundBox");

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
        }

        private void EnablePlayerInput() 
        {

        }

        public void OnPlayerJoined(PlayerInput playerInput)
        {
            if (playerOneInput == null)
            {
                playerOneInput = playerInput;
                playerOneCharacterImage.visible = true;
            }
            else if (playerTwoInput == null)
            {
                playerTwoInput = playerInput;
                playerTwoCharacterImage.visible = true;
                OnBothPlayersJoined();
            }
            else
            {
                Debug.LogError("Too many players joined, this should not be allowed by the PlayerInputManager.");
            }
            
            playerInput.SwitchCurrentActionMap("UI");
            playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
            
            
            print($"Player joined, {playerInput.user}");
        }

        private void OnBothPlayersJoined()
        {
            
        }
        
        #region Util

        private void VerboseLog(string msg)
        {
            if (verboseLogging) Debug.Log("CHARACTER_SELECT: " + msg);
        }
        
        #endregion
    }
}