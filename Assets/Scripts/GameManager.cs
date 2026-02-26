using System;
using System.Linq;
using Mirror;
using UnityEngine;

namespace DigDig2
{
    public class GameManager : Singleton<GameManager>
    {
        [Header("Player Prefabs")]
        [SerializeField] private CharacterType singleplayerStartingCharacter = CharacterType.Max;
        [SerializeField] private GameObject maxPrefab;
        [SerializeField] private GameObject miniPrefab;

        public enum CharacterType
        {
            Max,
            Mini,
        }

        public GameObject LocalPlayerObj
        {
            get
            {
                if (NetworkClient.localPlayer) return NetworkClient.localPlayer.gameObject;

                return null;
            }
        }

        public CharacterType currentCharacter { private set; get; }


        private void Start()
        {
            if (NetworkServer.active)
            {
                if (NetworkManager.singleton.IsMultiplayer) InitializeMultiplayer();
                else SingleplayerInitialize();
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
            currentCharacter = Enum.GetValues(typeof(CharacterType)).Cast<CharacterType>()
                    .SkipWhile(e => e != currentCharacter).Skip(1).First();

            GameObject newPrefab = GetCharacterPrefabFromCharacterType(currentCharacter);

            Vector3 lookVector = GetComponent<EntityCharacterController>().GetForwardVector();
            GameObject playerCharacter = Instantiate(newPrefab, transform.position, transform.rotation);
            playerCharacter.transform.LookAt(playerCharacter.transform.position + lookVector);
            NetworkServer.ReplacePlayerForConnection(NetworkServer.connections[0], playerCharacter, ReplacePlayerOptions.KeepAuthority);
            Destroy(gameObject);
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
