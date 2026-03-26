using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using DigDig2.CinemaCamera;
using DigDig2.CinemaCamera.CameraEffectors;
using DigDig2.Util;
using DigDig2.SaveSystem;
using DigDig2.UI.Controllers;
using DigDig2.Combat;
using DigDig2.Debugging;
using DigDig2.Input;
using DigDig2.Player;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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

        [SerializeField] private GameObject playerControllerPrefab;
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
        
        // Player
        [SerializeField] public PlayerController[] playerControllers = new PlayerController[2];
        public PlayerController PlayerOne => playerControllers[0];
        public PlayerController PlayerTwo => playerControllers[1];
        public int maxPlayerID;
        public int minisPlayerID;
        public PlayerController playerMax => playerControllers[maxPlayerID];
        public PlayerController playerMinis => playerControllers[minisPlayerID];

        public GameObject[] PlayerCharacterObjects
        {
            get
            {
                return playerControllers.Select(p => p.characterObject).ToArray();
            }
        }

        public UnityEvent<PlayerController> playerDeath = new();

        
        private PauseMenuController pauseMenuController;
        private GameHudController gameHudController;
        
        
        #region UnityCallbacks
        
        protected override void Awake()
        {
            base.Awake();
            
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
                savePointToStartAt.PlayMultiplayerStartSequence();
            }
            else
            {
                savePointToStartAt.PlaySingleplayerStartSequence();
                GameCamera.Instance.SetTargetRotation(savePointToStartAt.cameraYRotation);
            }
        }

        private void OnSavePointStartUpSequenceDone()
        {
            Debug.Log("GAME STARTED EVENT !!! ");
            InputManager.Instance.CurrentInputContext = gameInputContext;
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
                        crystal.GetComponent<Health>().death.AddListener((_) => SetHighestKilledCrystalIndex(gay));
                    }
                }
            }
        }
        
        #endregion

        public GameObject GetCharacterPrefabFromCharacterType(CharacterType characterType)
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
			LoadingScreenManager.Instance.LoadScene( 0 );
        }

        public void ReloadGameScene()
		{
			StartCoroutine( ReloadGameSceneAsync( ) );
		}

		private IEnumerator ReloadGameSceneAsync( )
		{
			yield return LoadingScreenManager.Instance.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
			StartGame();
		}

        public void RegisterCharacterDeath(GameObject characterObject)
        {
            
            for (int i = playerControllers.Length-1; i >= 0; i--)
            {
                PlayerController player = playerControllers[i];
                if (player.characterObject == characterObject)
                {
                    playerDeath.Invoke(player);
                }
            }
            
            bool boolplayerIsAlive = false;
            foreach (var player in playerControllers)
            {
                if (player.IsAlive) boolplayerIsAlive = true;
            }

            if (boolplayerIsAlive) ReloadGameScene();
        }

        #region Singleplayer

        public void InitializeSingleplayerCharacter(Vector3 spawnPosition, Quaternion spawnRotation)
        {
            if (IsMultiplayer)
            {
                BetterDebug.Log( "Tried to initialize singleplayer character in multiplayer mode, this is not allowed.", LogSeverity.Error );
                return;
            }

            PlayerController newPlayerController = Instantiate(playerControllerPrefab).GetComponent<PlayerController>();

            GameObject newCharacter = Instantiate( // SinglePlayerRef initialized in Restore state
                GetCharacterPrefabFromCharacterType(loadedGameManagerSaveData.singleplayerSelectedCharacter)
            );
            
            newCharacter.transform.SetParent(newPlayerController.transform);
            newPlayerController.SetCharacterObject(newCharacter); // MUST BE DONE FIRST
            
            newPlayerController.entityController.Teleport(spawnPosition, spawnRotation.eulerAngles.y);
            newPlayerController.health.death.AddListener(RegisterCharacterDeath);

            playerControllers[0] = newPlayerController;
                
            BetterDebug.Log( "Singleplayer Character Initialized!" );
        }

        #endregion

        #region Multiplayer

        public void RegisterMultiplayerPlayers(PlayerController playerOne, PlayerController playerTwo)
        {
            playerControllers[0] = playerOne;
            playerControllers[1] = playerTwo;

            maxPlayerID = playerOne.characterType == CharacterType.Max ? 0 : 1;
            minisPlayerID = playerOne.characterType == CharacterType.Minis ? 0 : 1;
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
            }

            InitializeTheLoadedSave();
        }


        private void InitializeTheLoadedSave()
        {
            // PlayerOne.characterType = loadedGameManagerSaveData.singleplayerSelectedCharacter;
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