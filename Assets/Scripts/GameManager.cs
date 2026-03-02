using Mirror;
using UnityEngine;

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

        [Header("Enemy Focusing")]
        [SerializeField] private GameObject focusIndicator;
        [Header("SavePoints")]
        [SerializeField] private SavePoint[] savePoints;

        public struct GameManagerGameSaveData
        {
            public CharacterType singleplayerSelectedCharacter;
            public int highestReachedSavePointIndex;
        }
        public GameManagerGameSaveData loadedGameManagerSaveData { get; private set; }

        public GameObject LocalPlayerObj
        {
            get
            {
                if (NetworkClient.localPlayer) return NetworkClient.localPlayer.gameObject;

                return null;
            }
        }

        public CharacterType currentCharacter { private set; get; } = CharacterType.Max;


        protected void Start()
        {
            
        }

        public override void OnStartServer()
        {
            SaveManager.Instance.RegisterSavable("GameManager", this, true);
            for (int i = 0; i < savePoints.Length; i++)
            {
                savePoints[i].GetComponent<SavePoint>().savePointReached.AddListener(() => SetHighestReachedSavePointIndex(i));
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

        #region Singleplayer

        public void InitializeSingleplayerCharacter(Vector3 spawnPosition, Quaternion spawnRotation)
        {
            if (NetworkManager.singleton.IsMultiplayer)
            {
                Debug.LogError("Tried to initialize singleplayer character in multiplayer mode. This is not allowed.");
                return;
            }
            Debug.Log("GAMEMANAGER: Initializing Singleplayer Characters...");
            GameObject playerCharacter = Instantiate(GetCharacterPrefabFromCharacterType(loadedGameManagerSaveData.singleplayerSelectedCharacter), spawnPosition, spawnRotation);
            NetworkServer.AddPlayerForConnection(NetworkServer.localConnection, playerCharacter);
            Debug.Log("GAMEMANAGER: Singleplayer Characters Initialized!");
        }

        public void SingleplayerSwitchCharacter()
        {
            // Harvvest old player data
            EntityCharacterController oldPlayerEntityCharacterController = LocalPlayerObj.GetComponent<EntityCharacterController>();
            Vector3 oldPlayerPos = LocalPlayerObj.transform.position;
            Vector3 oldPlayerLookVector = oldPlayerEntityCharacterController.GetForwardVector();
            Vector3 oldPlayerInputMoveVector = oldPlayerEntityCharacterController.inputMoveVector;

            // Kill old player
            Destroy(LocalPlayerObj);

            // Switch Logic
            currentCharacter = CharacterType.Max == currentCharacter ? CharacterType.Mini : CharacterType.Max;

            // Spawn new player
            GameObject newPrefab = GetCharacterPrefabFromCharacterType(currentCharacter);
            GameObject playerCharacter = Instantiate(newPrefab, oldPlayerPos, Quaternion.identity);
            EntityCharacterController playerEntityCharacterController = playerCharacter.GetComponent<EntityCharacterController>();
            playerEntityCharacterController.LookTowards(playerCharacter.transform.position + oldPlayerLookVector, false);
            playerEntityCharacterController.inputMoveVector = oldPlayerInputMoveVector;
            NetworkServer.ReplacePlayerForConnection(NetworkServer.connections[0], playerCharacter, ReplacePlayerOptions.KeepAuthority);
            
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

        public void FocusOnPosition(Vector3 position)
        {
            focusIndicator.transform.position = Camera.main.WorldToScreenPoint(position);
        }
        public void SetFocusIndicatorVibility(bool visible)
        {
            focusIndicator.SetActive(visible);
        }

        #endregion
        #region Saving and Loading 

        public object CollectData()
        {
            return loadedGameManagerSaveData;
        }

        public void RestoreState(object dataObject)
        {
            if (dataObject == null)
            {
                loadedGameManagerSaveData = new GameManagerGameSaveData
                {
                    singleplayerSelectedCharacter = defaultSingleplayerCharacter,
                    highestReachedSavePointIndex = 0,
                };
            }
            else
            {
                loadedGameManagerSaveData = (GameManagerGameSaveData)dataObject;
                currentCharacter = loadedGameManagerSaveData.singleplayerSelectedCharacter;
            }
        }

        public void SetHighestReachedSavePointIndex(int index)
        {
            var tempData = loadedGameManagerSaveData;
            tempData.highestReachedSavePointIndex = index;
            loadedGameManagerSaveData = tempData;
        }

        #endregion
    }
}
