using System;
using System.Linq;
using Mirror;
using UnityEngine;

namespace DigDig2
{

    public enum CharacterType
    {
        Max,
        Mini,
    }
    public class GameManager : Singleton<GameManager>
    {
        [Header("Player Prefabs")]
        [SerializeField] private CharacterType singleplayerStartingCharacter = CharacterType.Max;
        [SerializeField] private GameObject maxPrefab;
        [SerializeField] private GameObject miniPrefab;



        public GameObject LocalPlayerObj
        {
            get
            {
                if (NetworkClient.localPlayer) return NetworkClient.localPlayer.gameObject;

                return null;
            }
        }

        public CharacterType currentCharacter { private set; get; } = CharacterType.Max;


        private void Start()
        {
            if (NetworkServer.active)
            {
                if (!NetworkManager.singleton.IsMultiplayer) SingleplayerInitialize();
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

        private void SingleplayerInitialize()
        {
            Debug.Log("Initializing Singleplayer...");
            GameObject playerCharacter = Instantiate(GetCharacterPrefabFromCharacterType(singleplayerStartingCharacter));
            NetworkServer.ReplacePlayerForConnection(NetworkServer.connections[0], playerCharacter, ReplacePlayerOptions.KeepAuthority);
            Debug.Log("Singleplayer Initialization Finished!");
        }

        public void SingleplayerSwitchCharacter()
        {
            // Harvvest old player data
            Vector3 oldPlayerLookVector = LocalPlayerObj.GetComponent<EntityCharacterController>().GetForwardVector();
            Debug.Log(oldPlayerLookVector);
            Vector3 oldPlayerPos = LocalPlayerObj.transform.position;

            // Kill old player
            Destroy(LocalPlayerObj);

            // Switch Logic
            currentCharacter = CharacterType.Max == currentCharacter ? CharacterType.Mini : CharacterType.Max;

            // Spawn new player
            GameObject newPrefab = GetCharacterPrefabFromCharacterType(currentCharacter);
            GameObject playerCharacter = Instantiate(newPrefab, oldPlayerPos, Quaternion.identity);
            EntityCharacterController playerEntityCharacterController = playerCharacter.GetComponent<EntityCharacterController>();
            playerEntityCharacterController.LookTowards(playerCharacter.transform.position + oldPlayerLookVector, false);
            NetworkServer.ReplacePlayerForConnection(NetworkServer.connections[0], playerCharacter, ReplacePlayerOptions.KeepAuthority);
            
        }

        #endregion

        #region Multiplayer

        private void InitializeMultiplayer()
        {
            Debug.Log("Initializing Multiplayer...");
            InitializeMultiplayerPlayers();
            Debug.Log("Multiplayer Initialization Finished!");
        }

        private void InitializeMultiplayerPlayers()
        {
            GameObject maxPlayerCharacter = Instantiate(maxPrefab);
            NetworkServer.ReplacePlayerForConnection(NetworkManager.singleton.MaxPlayerConnection, maxPlayerCharacter, ReplacePlayerOptions.KeepAuthority);

            GameObject miniPlayerCharacter = Instantiate(miniPrefab);
            NetworkServer.ReplacePlayerForConnection(NetworkManager.singleton.MiniPlayerConnection, miniPlayerCharacter, ReplacePlayerOptions.KeepAuthority);
        }

        #endregion
    }
}
