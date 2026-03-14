using System;
using DigDig2.CinemaCamera;
using DigDig2.Util;
using DigDig2.SaveSystem;
using DigDig2.UI.Controllers;
using DigDig2.Combat;
using DigDig2.Debugging;
using DigDig2.Input;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace DigDig2.Game
{
    public enum CharacterType
    {
        Max,
        Minis,
    }

    public class GameManager : Singleton<GameManager>, ISaveable
    {
        [Header("Player Prefabs")]
        [SerializeField] private CharacterType defaultSingleplayerCharacter = CharacterType.Max;
        [SerializeField] private GameObject maxPrefab;
        [SerializeField] private GameObject miniPrefab;

        [Header("Save Points")]
        [SerializeField] private SavePoint[] savePoints;
        [SerializeField] private Crystal[] crystals;
        
        [SerializeField] public UnityEvent<CharacterType, GameObject> characterSwitched = new();
        [SerializeField] public UnityEvent gameStarted = new();

        public bool Paused
        {
            get
            {
                return pauseMenuController.Paused;
            }
        }
        [SerializeField] public UnityEvent<bool> pauseStateChanged;
        
        public bool IsMultiplayer => SaveManager.Instance.IsMultiplayer;

        [Header( "Input Context" )]
		[SerializeField] private InputContext gameInputContext;
		[SerializeField] private InputContext pauseMenuInputContext;

        
        // Saving
        [Serializable]
        public struct GameManagerGameSaveData
        {
            public CharacterType singleplayerSelectedCharacter;
            public int highestReachedSavePointIndex;
            public int highestKilledCrystal;
        }
        public GameManagerGameSaveData loadedGameManagerSaveData;

        // Character
        public CharacterType singlePlayerCurrentCharacter { private set; get; } = CharacterType.Max;
        
        // Player
        public GameObject[] players = new GameObject[2];
        public GameObject PlayerOneCharacter => players[0];
        public GameObject PlayerTwoCharacter => players[1];
        public int maxPlayerID;
        public int minisPlayerID;

        
        private PauseMenuController pauseMenuController;
        private GameHudController gameHudController;

        // Input
        public  PlayerInput playerOneInput;
        public  PlayerInput playerTwoInput;
        
        #region UnityCallbacks

        // Character
        public CharacterType currentCharacter { private set; get; } = CharacterType.Max;


        protected override void Awake()
        {
            pauseMenuController = GetComponentInChildren<PauseMenuController>();
            pauseMenuController.stateChanged.AddListener((bool state) =>
            {
                pauseStateChanged.Invoke(state);
            });

            gameHudController = GetComponentInChildren<GameHudController>();
        }

        public void Start()
        {
            SaveManager.Instance.RegisterSavable("GameManager", this, true);
            InputManager.Instance.CurrentInputContext = gameInputContext;
            
            StartGame();
        }
        
        void OnDestroy()
        {
            if (SaveManager.Instance) SaveManager.Instance.Reset();
        }
        
        #endregion
        #region StartGame
        
        private void StartGame()
        {
            InitializeSavePoint();
            KillAlreadyKilledCrystals();
            
            Debug.Log(loadedGameManagerSaveData.highestReachedSavePointIndex);
            SavePoint savePointToStartAt = savePoints[loadedGameManagerSaveData.highestReachedSavePointIndex];
            savePointToStartAt.startSequenceDone.AddListener(OnSavePointStartUpSequenceDone);

            if (IsMultiplayer)
            {
                savePointToStartAt.ServerStartMultiplayerStartSequence();
            }
            else
            {
                savePointToStartAt.ServerStartSingleplayerStartSequence();
                GameCamera.Instance.SetTargetRotation(savePointToStartAt.cameraYRotation, true);
            }
        }

        private void OnSavePointStartUpSequenceDone()
        {
            Debug.Log("GAME STARTED EVENT !!! ");
            gameStarted.Invoke();
        }

        private void InitializeSavePoint()
        {
            Debug.Log($"savePointReached in Loaded Save {loadedGameManagerSaveData.highestReachedSavePointIndex}");

            for (int i = 0; i < savePoints.Length; i++)
            {
                if (savePoints[i])
                {
                    SavePoint savePoint = savePoints[i];
                    int gay = i;
                    savePoint.SetSpawnPointReached(i <= loadedGameManagerSaveData.highestReachedSavePointIndex);
                    savePoint.savePointReached.AddListener(() => SetHighestReachedSavePointIndex(gay));
                    Debug.Log(savePoint.name + " " + i);
                }
            }
        }

        private void KillAlreadyKilledCrystals()
        {
			BetterDebug.Log( $"Crystals killed in Loaded Save {loadedGameManagerSaveData.highestKilledCrystal}" );

            for (int i = 0; i < crystals.Length; i++)
            {
                if (crystals[i])
                {
                    Crystal crystal = crystals[i];
                    int gay = i;
                    if (i <= loadedGameManagerSaveData.highestKilledCrystal) 
                    {
                        Destroy(crystal.gameObject);
                        Debug.Log($"crystal [{i}] KILLED");
                    }
                    else 
                    {
                        crystal.GetComponent<Health>().death.AddListener(() => SetHighestKilledCrystalIndex(gay));
                    }
                }
            }
        }
        
        #endregion

        private GameObject GetCharacterPrefabFromCharacterType(CharacterType characterType)
        {
            return characterType switch
            {
                CharacterType.Max => maxPrefab,
                CharacterType.Minis => miniPrefab,
                _ => null,
            };
        }

        public void SaveAndLoadMainMenu()
        {
            SaveManager.Instance.SaveAllAndWriteToFile();
            SceneManager.LoadScene(0);
        }

        public async void ReloadGameScene()
        {
            await SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            StartGame();
        }

        #region Singleplayer

        public void InitializeSingleplayerCharacter(Vector3 spawnPosition, Quaternion spawnRotation)
        {
            if (IsMultiplayer)
            {
                BetterDebug.Log( "Tried to initialize singleplayer character in multiplayer mode, this is not allowed.", LogSeverity.Error );
                return;
            }
			
            GameObject playerCharacter = Instantiate(
                GetCharacterPrefabFromCharacterType(loadedGameManagerSaveData.singleplayerSelectedCharacter),
                spawnPosition,
                spawnRotation
            );

            players[0] = playerCharacter;
            BetterDebug.Log( "Singleplayer Character Initialized!" );
        }

        public void SingleplayerSwitchCharacter()
        {
            if (IsMultiplayer)
            {
                BetterDebug.Log( "Tried to switch character in multiplayer mode, this is not allowed.", LogSeverity.Error );
            }

            // Harvest old player data
            EntityCharacterController oldPlayerEntityCharacterController = PlayerOneCharacter.GetComponent<EntityCharacterController>();
            Health oldPlayerHealthComponent = PlayerOneCharacter.GetComponent<Health>();

            Vector3 oldPlayerPos = PlayerOneCharacter.transform.position;

            Vector3 oldPlayerLookVector = oldPlayerEntityCharacterController.GetForwardVector();
            Vector3 oldPlayerInputMoveVector = oldPlayerEntityCharacterController.inputMoveVector;

            int oldHealthPoints = oldPlayerHealthComponent.HealthPoints;

            // Kill old player
            Destroy(PlayerOneCharacter);

            // Switch Logic
            singlePlayerCurrentCharacter = CharacterType.Max == singlePlayerCurrentCharacter ? CharacterType.Minis : CharacterType.Max;

            // Spawn new player
            GameObject newPrefab = GetCharacterPrefabFromCharacterType(singlePlayerCurrentCharacter);
            GameObject playerCharacter = Instantiate(newPrefab, oldPlayerPos, Quaternion.identity);

            
            // Inject Old Player percistance data
            EntityCharacterController playerEntityCharacterController = playerCharacter.GetComponent<EntityCharacterController>();
            Health playerHealthComponent = playerCharacter.GetComponent<Health>();
            
            playerEntityCharacterController.LookTowards(playerCharacter.transform.position + oldPlayerLookVector, false);
            playerEntityCharacterController.inputMoveVector = oldPlayerInputMoveVector;
            
            playerHealthComponent.HealthPoints = oldHealthPoints;
            
            players[0] = playerCharacter;
            characterSwitched.Invoke(currentCharacter, playerCharacter);
        }

        #endregion

        #region Multiplayer

        public void RegisterMultiplayerPlayers(GameObject playerOne, CharacterType playerOneCharacterType, GameObject playerTwo, CharacterType playerTwoCharacterType)
        {
            players[0] = playerOne;
            players[1] = playerTwo;

            maxPlayerID = playerOneCharacterType == CharacterType.Max ? 0 : 1;
            minisPlayerID = playerOneCharacterType == CharacterType.Minis ? 0 : 1;
            Debug.Log("GAMEMANAGER: Multiplayer Characters registered!");
        }

        #endregion
        #region Saving and Loading

        public object CollectData()
        {
            return loadedGameManagerSaveData;
        }

        public void RestoreState(object dataObject)
        {
            Debug.Log("SUNG JINWOOOO");
            if (dataObject == null)
            {
                Debug.Log("SUNG KIM");
                loadedGameManagerSaveData = new GameManagerGameSaveData
                {
                    singleplayerSelectedCharacter = defaultSingleplayerCharacter,
                    highestReachedSavePointIndex = 0,
                    highestKilledCrystal = -1,
                };
            }
            else
            {
                Debug.Log($"GAAAY {dataObject}");
                try
                {
                    loadedGameManagerSaveData = (GameManagerGameSaveData)dataObject;
                }
                catch
                {
                    loadedGameManagerSaveData = JsonConvert.DeserializeObject<GameManagerGameSaveData>(dataObject.ToString());
                }
                
                
                
                singlePlayerCurrentCharacter = loadedGameManagerSaveData.singleplayerSelectedCharacter;
                Debug.Log(loadedGameManagerSaveData.highestReachedSavePointIndex);
            }
        }

        public void SetHighestReachedSavePointIndex(int index)
        {
            loadedGameManagerSaveData.highestReachedSavePointIndex = index;
        }

        public void SetHighestKilledCrystalIndex(int index)
        {
            loadedGameManagerSaveData.highestKilledCrystal = index;
        }

        #endregion

        #region Pausing

        public void Pause()
        {
            pauseMenuController.Open();
        }

        public void Resume()
        {
            pauseMenuController.Close();
        }

        #endregion
    }
}
