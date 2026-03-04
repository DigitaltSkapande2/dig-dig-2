using System;
using System.Collections.Generic;
using Mirror;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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

        [Header("SavePoints")]
        [SerializeField] private SavePoint[] savePoints;

        [Serializable]
        public struct GameManagerGameSaveData
        {
            public CharacterType singleplayerSelectedCharacter;
            public int highestReachedSavePointIndex;
        }
        public GameManagerGameSaveData loadedGameManagerSaveData;
        [SerializeField] public UnityEvent<CharacterType, GameObject> characterSwitched;

        public bool Paused
        {
            get
            {
                return pauseMenuController.Paused;
            }
        }
        [SerializeField] public UnityEvent<bool> pauseStateChanged;

        private PauseMenuController pauseMenuController;
        private GameHudController gameHudController;



        public GameObject LocalPlayerObj
        {
            get
            {
                if (NetworkClient.localPlayer) return NetworkClient.localPlayer.gameObject;

                return null;
            }
        }

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
            if (!isServer) return;
            SaveManager.Instance.RegisterSavable("GameManager", this, true);
            print(loadedGameManagerSaveData.highestReachedSavePointIndex);
            for (int i = 0; i < savePoints.Length; i++)
            {
                if (savePoints[i].TryGetComponent(out SavePoint spawnpoint))
                {
                    int gay = i;
                    spawnpoint.savePointReached.AddListener(() => SetHighestReachedSavePointIndex(gay));
                    spawnpoint.RcpSetSpawnPointReached(i <= loadedGameManagerSaveData.highestReachedSavePointIndex);
                }
            }

            SavePoint savePointToStartAt = savePoints[loadedGameManagerSaveData.highestReachedSavePointIndex];

            if (NetworkManager.singleton.IsMultiplayer)
            {
                savePointToStartAt.ServerStartMultiplayerStartSequence();
            }
            else
            {
                savePointToStartAt.ServerStartSingleplayerStartSequence();
            }
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

        public void SaveAndExit()
        {

            if (NetworkServer.active)
            {
                SaveManager.Instance.SaveAllAndWriteToFile();
                NetworkManager.singleton.StopHost();
            }
            else NetworkManager.singleton.StopClient();

            SceneManager.LoadScene(0);
        }

        #region Singleplayer

        public void InitializeSingleplayerCharacter(Vector3 spawnPosition, Quaternion spawnRotation)
        {
            if (NetworkManager.singleton.IsMultiplayer)
            {
                Debug.LogError("Tried to initialize singleplayer character in multiplayer mode, this is not allowed.");
                return;
            }
            Debug.Log("GAMEMANAGER: Initializing Singleplayer Characters...");
            GameObject playerCharacter = Instantiate(GetCharacterPrefabFromCharacterType(loadedGameManagerSaveData.singleplayerSelectedCharacter), spawnPosition, spawnRotation);
            if (NetworkServer.localConnection.identity) NetworkServer.ReplacePlayerForConnection(NetworkServer.localConnection, playerCharacter, ReplacePlayerOptions.Destroy);
            else NetworkServer.AddPlayerForConnection(NetworkServer.localConnection, playerCharacter);
            Debug.Log("GAMEMANAGER: Singleplayer Characters Initialized!");
        }

        public void SingleplayerSwitchCharacter()
        {
            if (NetworkManager.singleton.IsMultiplayer)
            {
                Debug.LogError("Tried to switch character in multiplayer mode, this is not allowed.");
            }

            // Harvest old player data
            EntityCharacterController oldPlayerEntityCharacterController = LocalPlayerObj.GetComponent<EntityCharacterController>();
            Health oldPlayerHealthComponent = LocalPlayerObj.GetComponent<Health>();

            Vector3 oldPlayerPos = LocalPlayerObj.transform.position;

            Vector3 oldPlayerLookVector = oldPlayerEntityCharacterController.GetForwardVector();
            Vector3 oldPlayerInputMoveVector = oldPlayerEntityCharacterController.inputMoveVector;

            int oldHealthPoints = oldPlayerHealthComponent.HealthPoints;

            // Kill old player
            Destroy(LocalPlayerObj);

            // Switch Logic
            currentCharacter = CharacterType.Max == currentCharacter ? CharacterType.Mini : CharacterType.Max;

            // Spawn new player
            GameObject newPrefab = GetCharacterPrefabFromCharacterType(currentCharacter);
            GameObject playerCharacter = Instantiate(newPrefab, oldPlayerPos, Quaternion.identity);

            EntityCharacterController playerEntityCharacterController = playerCharacter.GetComponent<EntityCharacterController>();
            Health playerHealthComponent = playerCharacter.GetComponent<Health>();

            playerEntityCharacterController.LookTowards(playerCharacter.transform.position + oldPlayerLookVector, false);
            playerEntityCharacterController.inputMoveVector = oldPlayerInputMoveVector;

            playerHealthComponent.HealthPoints = oldHealthPoints;

            NetworkServer.ReplacePlayerForConnection(NetworkServer.connections[0], playerCharacter, ReplacePlayerOptions.KeepAuthority);

            characterSwitched.Invoke(currentCharacter, playerCharacter);
        }

        #endregion

        #region Multiplayer

        private void InitializeMultiplayerPlayers()
        {
            Debug.Log("GAMEMANAGER: Initializing Multiplayer Characters...");
            GameObject maxPlayerCharacter = Instantiate(maxPrefab);
            NetworkServer.ReplacePlayerForConnection(NetworkManager.singleton.MaxPlayerConnection, maxPlayerCharacter, ReplacePlayerOptions.KeepAuthority);

            GameObject miniPlayerCharacter = Instantiate(miniPrefab);
            NetworkServer.ReplacePlayerForConnection(NetworkManager.singleton.MiniPlayerConnection, miniPlayerCharacter, ReplacePlayerOptions.KeepAuthority);
            Debug.Log("GAMEMANAGER: Multiplayer Characters Initialized!");
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
                loadedGameManagerSaveData = new GameManagerGameSaveData
                {
                    singleplayerSelectedCharacter = defaultSingleplayerCharacter,
                    highestReachedSavePointIndex = 0,
                };
                Debug.Log("SUNG KIM");
            }
            else
            {
                loadedGameManagerSaveData = JsonConvert.DeserializeObject<GameManagerGameSaveData>(dataObject.ToString());
                currentCharacter = loadedGameManagerSaveData.singleplayerSelectedCharacter;
                Debug.Log($"GAAAY {loadedGameManagerSaveData.highestReachedSavePointIndex}");
            }
        }

        public void SetHighestReachedSavePointIndex(int index)
        {
            loadedGameManagerSaveData.highestReachedSavePointIndex = index;
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
