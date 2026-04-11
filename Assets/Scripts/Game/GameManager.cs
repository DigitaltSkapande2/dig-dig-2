using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using DigDig2.CinemaCamera;
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
using UnityEngine.SceneManagement;

namespace DigDig2.Game
{
    public enum CharacterType
    {
        Max,
        Minis,
    }

    public class GameManager : Singleton<GameManager>
    {
        [Header("Player Prefabs")]
        [SerializeField] private CharacterType defaultSingleplayerCharacter = CharacterType.Max;

        [SerializeField] private GameObject playerControllerPrefab;
        [SerializeField] private GameObject maxPrefab;
        [SerializeField] private GameObject miniPrefab;
        
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
        
        public bool IsMultiplayer => SaveManager.Instance.isMultiplayer;

        [Header( "Input Context" )]
		[SerializeField] private InputContext gameInputContext;
		[SerializeField] private InputContext pauseMenuInputContext;
        
        // Player
        [SerializeField] public PlayerController[] playerControllers = new PlayerController[2];
        public PlayerController PlayerOne => playerControllers[0];
        public PlayerController PlayerTwo => playerControllers[1];
        [NonSerialized] public int maxPlayerID = -1;
        [NonSerialized] public int maxInputPlayerIndex = -1;
        [NonSerialized] public int minisPlayerID = -1;
        [NonSerialized] public int minisInputPlayerIndex = -1;
        [NonSerialized] public bool hasCharacterBeenSelected = false;
        public PlayerController playerMax => playerControllers[maxPlayerID];
        public PlayerController playerMinis => playerControllers[minisPlayerID];

        public GameObject[] PlayerCharacterObjects
        {
            get
            {
                return playerControllers.Select(p => p.characterObject).ToArray();
            }
        }
        
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
        
        
        #endregion
        #region StartGame
        
        public void StartGame()
        {
            BetterDebug.Log("Game Started!");
            InputManager.Instance.CurrentInputContext = gameInputContext;
            gameStarted.Invoke();
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
            Destroy(gameObject);
            Destroy(SaveManager.Instance);
        }

        public async UniTask ReloadGameScene()
        {
            SaveManager.Instance.Reset();
            playerControllers[0] = null;
            playerControllers[1] = null;
            await LoadingScreenManager.Instance.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }

        public void RegisterCharacterDeath()
        {
            bool playerIsAlive = false;
            foreach (PlayerController player in playerControllers)
            {
                if (player != null &&  player.IsAlive)
                {
                    BetterDebug.Log(player.name  + " is alive!");
                    playerIsAlive = true;
                }
            }
            
            BetterDebug.Log("Character has died, and any player alive = " + playerIsAlive);
            if (!playerIsAlive) ReloadGameScene().Forget();
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
                GetCharacterPrefabFromCharacterType(defaultSingleplayerCharacter)
            );
            
            newCharacter.transform.SetParent(newPlayerController.transform);
            newPlayerController.SetCharacterObject(newCharacter); // MUST BE DONE FIRST
            
            newPlayerController.entityController.Teleport(spawnPosition, spawnRotation.eulerAngles.y);

            playerControllers[0] = newPlayerController;
                
            BetterDebug.Log( "Singleplayer Character Initialized!" );
        }

        #endregion

        #region Multiplayer

        public void RegisterMultiplayerPlayers(PlayerController playerOne, int playerOneInputPlayerIndex, PlayerController playerTwo, int playerTwoInputPlayerIndex)
        {
            playerControllers[0] = playerOne;
            playerControllers[1] = playerTwo;
            hasCharacterBeenSelected = true;

            if (playerOne.characterType == CharacterType.Max)
            {
                maxPlayerID = 0;
                maxInputPlayerIndex = playerOneInputPlayerIndex;
                minisPlayerID = 1;
                minisInputPlayerIndex = playerTwoInputPlayerIndex;
            }
            else
            {
                maxPlayerID = 1;
                maxInputPlayerIndex = playerTwoInputPlayerIndex;
                minisPlayerID = 0;
                minisInputPlayerIndex = playerOneInputPlayerIndex;
            }
            Debug.Log("GAMEMANAGER: Multiplayer Characters registered!");
        }

        #endregion
        
        #region Player Spawning
        
        
            
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