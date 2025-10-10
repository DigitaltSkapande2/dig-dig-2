using Mirror;
using UnityEngine;

namespace DigDig2
{
    public class OurNetworkManager : NetworkManager
    {
        public static OurNetworkManager instance;
        [SerializeField] GameObject player1Prefab;
        [SerializeField] GameObject player2Prefab;

        [SerializeField] public bool isSinglePlayer = false;
        private PlayerCharacterInputController[] playerControllers = new PlayerCharacterInputController[2];
        private int singleplayerCharacterFocus = 0;

        #region Overrides

        public override void Awake()
        {

            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            base.Awake();
        }

        public override void Start()
        {
            base.Start();

        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            Transform startPos = GetStartPosition();
            GameObject player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            // instantiating a "Player" prefab gives it the name "Player(clone)"
            // => appending the connectionId is WAY more useful for debugging!
            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, player);
        }

        #endregion


        [Server]
        private void InitializePlayers()
        {
            playerControllers[0] = Instantiate(player1Prefab).GetComponent<PlayerCharacterInputController>();
            playerControllers[1] = Instantiate(player2Prefab).GetComponent<PlayerCharacterInputController>();

            playerControllers[singleplayerCharacterFocus].isSinglePlayerFocus = true;
        }

        public void ToggleSinglePlayerFocus()
        {
            playerControllers[singleplayerCharacterFocus].isSinglePlayerFocus = false;
            singleplayerCharacterFocus = (singleplayerCharacterFocus + 1) % (playerControllers.Length - 1);
            playerControllers[singleplayerCharacterFocus].isSinglePlayerFocus = true;
        }



        public PlayerCharacterInputController[] GetSinglePlayerCharacters()
        {
            return playerControllers;
        }

        public PlayerCharacterInputController GetFocusedSinglePlayerCharacter()
        {
            return playerControllers[singleplayerCharacterFocus];
        }
    }
}
