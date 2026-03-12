using System;
using DigDig2.CinemaCamera;

using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace DigDig2
{
    public enum CharacterType
    {
        Max,
        Mini,
    }

    public class GameManager : Singleton<GameManager>, ISaveable
    {
        [Header("Player Prefabs")]
        [SerializeField] private CharacterType defaultSingleplayerCharacter = CharacterType.Max;
        [SerializeField] private GameObject maxPrefab;
        [SerializeField] private GameObject miniPrefab;
        [SerializeField] private GameObject singlePlayerInputRoot;

        [Header("SavePoints")]
        [SerializeField] private SavePoint[] savePoints;
        [SerializeField] private Crystal[] crystals;
        
        [SerializeField] public UnityEvent<CharacterType, GameObject> characterSwitched;
        [SerializeField] public UnityEvent<bool> gameStarted;

        public bool Paused
        {
            get
            {
                return pauseMenuController.Paused;
            }
        }
        [SerializeField] public UnityEvent<bool> pauseStateChanged;
        
        public bool IsMultiplayer => SaveManager.Instance.IsMultiplayer;


        
        // Saving
        [Serializable]
        public struct GameManagerGameSaveData
        {
            public CharacterType singleplayerSelectedCharacter;
            public int highestReachedSavePointIndex;
            public int highestKilledCrystal;
        }
        public GameManagerGameSaveData loadedGameManagerSaveData;

        // Player
        public GameObject[] players = new GameObject[2];
        public GameObject PlayerOneCharacter
        {
            get
            {
#if UNITY_EDITOR
                if (players[0]) return players[0];
                //else Debug.LogError("No Players have been initialized");
                #else
                    return players[0];
                #endif

                return null;
            }
        }

        public GameObject PlayerTwoCharacter
        {
            get
            {
                if (players[1]) return players[1];
                else Debug.LogAssertion("No Players have been initialized");

                return null;
            }
        }
        
        private PauseMenuController pauseMenuController;
        private GameHudController gameHudController;

        // Input
        public  PlayerInput playerOneInput;
        public  PlayerInput playerTwoInput;

        // Character
        public CharacterType currentCharacter { private set; get; } = CharacterType.Max;


        private void Awake()
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
            
            StartGame();
        }
        
        private void StartGame()
        {
            InitializeSavePoint();
            KillAlreadyKilledCrystals();
            
            Debug.Log(loadedGameManagerSaveData.highestReachedSavePointIndex);
            SavePoint savePointToStartAt = savePoints[loadedGameManagerSaveData.highestReachedSavePointIndex];

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
            Debug.Log($"Crystals killed in Loaded Save {loadedGameManagerSaveData.highestKilledCrystal}");

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

        void OnDestroy()
        {
            if (SaveManager.Instance) SaveManager.Instance.Reset();
        }

        private GameObject GetCharacterPrefabFromCharacterType(CharacterType characterType)
        {
            return characterType switch
            {
                CharacterType.Max => maxPrefab,
                CharacterType.Mini => miniPrefab,
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
                Debug.LogError("Tried to initialize singleplayer character in multiplayer mode, this is not allowed.");
                return;
            }

            playerOneInput = Instantiate(singlePlayerInputRoot).GetComponent<PlayerInput>();
            GameObject playerCharacter = Instantiate(
                GetCharacterPrefabFromCharacterType(loadedGameManagerSaveData.singleplayerSelectedCharacter),
                spawnPosition,
                spawnRotation,
                playerOneInput.transform
            );

            players[0] = playerCharacter;
            Debug.Log("GAMEMANAGER: Singleplayer Character Initialized!");
        }

        public void SingleplayerSwitchCharacter()
        {
            if (IsMultiplayer)
            {
                Debug.LogError("Tried to switch character in multiplayer mode, this is not allowed.");
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
            currentCharacter = CharacterType.Max == currentCharacter ? CharacterType.Mini : CharacterType.Max;

            // Spawn new player
            GameObject newPrefab = GetCharacterPrefabFromCharacterType(currentCharacter);
            GameObject playerCharacter = Instantiate(newPrefab, oldPlayerPos, Quaternion.identity, playerOneInput.transform);
            
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

        public void RegisterMultiplayerPlayers(GameObject playerOne, GameObject playerTwo)
        {
            players[0] = playerOne;
            players[1] = playerTwo;
            Debug.Log("GAMEMANAGER: Multiplayer Characters registered!");
        }

        #endregion

        #region Enemy Focusing

        public void FocusOnPosition(bool visible, Vector3 position)
        {
            gameHudController.UpdateFocusTarget(visible, position);
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
                
                
                
                currentCharacter = loadedGameManagerSaveData.singleplayerSelectedCharacter;
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