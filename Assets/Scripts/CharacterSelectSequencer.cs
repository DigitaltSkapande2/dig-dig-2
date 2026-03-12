
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.InputSystem;


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
        [SerializeField] private Button switchCharacterButton;
        [SerializeField] private Button startButton;
        [SerializeField] private TMP_Text maxNameplateText;
        [SerializeField] private TMP_Text minisNameplateText;
        [Header("Debug")]
        [SerializeField] bool verboseLogging = false;
        [FormerlySerializedAs("hostIsMax")] [SerializeField] private bool PlayerOneIsMax = true;
        
        private GameObject maxDummyInstance;
        private GameObject minisDummyInstance;
        
        PlayerInputManager playerInputManager;
        
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
        }

        private void Start()
        {
            maxDummyInstance = Instantiate(maxPrefab, maxSpawnPoint.position, maxSpawnPoint.rotation);
            minisDummyInstance = Instantiate(minisPrefab, minisSpawnPoint.position, minisSpawnPoint.rotation);

            switchCharacterButton.interactable = true;
            startButton.interactable = true;

            switchCharacterButton.onClick.AddListener(OnSwitchCharacterButtonClicked);
            startButton.onClick.AddListener(OnStartButtonClicked);
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
        }

        private void EnablePlayerInput() 
        {
            GameManager.Instance.PlayerOneCharacter.GetComponent<EntityCharacterController>().Frozen = false;
        }

        public void OnPlayerJoined()
        {
            print($"Player joined");
        }
        
        #region Util

        private void VerboseLog(string msg)
        {
            if (verboseLogging) Debug.Log("CHARACTER_SELECT: " + msg);
        }
        
        #endregion
    }
}