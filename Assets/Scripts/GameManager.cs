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

        public enum CharacterType {
            Max,
            Mini,
        }
        


        public GameObject CurrentCharacter
        {
            get
            {
                if (NetworkClient.localPlayer) return NetworkClient.localPlayer.gameObject;

                return null;
            }
        }



        private void Start()
        {
            if (NetworkServer.active)
            {
                if (NetworkManager.singleton.IsMultiplayer) InitializeMultiplayer();
                else InitializeSingleplayer();
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

        private void InitializeSingleplayer()
        {
            Debug.Log("Initializing Singleplayer...");
            GameObject playerCharacter = Instantiate(GetCharacterPrefabFromCharacterType(singleplayerStartingCharacter));
            NetworkServer.ReplacePlayerForConnection(NetworkServer.connections[0], playerCharacter, ReplacePlayerOptions.KeepAuthority);
            Debug.Log("Singleplayer Initialization Finished!");
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
