using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace DigDig2
{

    // HANDELS STUFF SPECIFIC TO GAME SCENE/SEQUENCE
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] GameObject debugStartGameContainer;

        [SerializeField] bool startSinglePlayerOnStart = false;

        [Header("Character Select Screen")]
        [SerializeField] GameObject characterSelectScreenContainer;
        [SerializeField] Button selectMinisButton;
        [SerializeField] Button selectMaxButton;
        [SerializeField] Button readyButton;

        [Header("Player")]
        [SerializeField] GameObject playerPrefab;

        Dictionary<int, PlayerCharacterInputController> playerInputToNetworkPlayerMap = new Dictionary<int, PlayerCharacterInputController>();


        OurNetworkManager networkManager;

        // Flags
        bool hasCharacterSelectScreenShown = false;

        void Start()
        {
            networkManager = OurNetworkManager.singleton;

            if (startSinglePlayerOnStart)
            {
                Invoke(nameof(StartShit), 0.5f);
                return;
            }

            if (NetworkServer.active)
            {
                debugStartGameContainer.SetActive(false);
                characterSelectScreenContainer.SetActive(true);
            }
        }
        
        void StartShit()
        {
            InitializePlayers();
            debugStartGameContainer.SetActive(false);
        }

        // Called by the DebugStart Game Screen
        public void DebugHostGame()
        {
            networkManager.StartHost();
            ShowCharacterSelectScreen();
            debugStartGameContainer.SetActive(false);
        }

        // Called by the DebugStart Game Screen
        public void DebugTryJoinGame()
        {
            networkManager.StartClient();
            ShowCharacterSelectScreen();
            debugStartGameContainer.SetActive(false);
        }

        public void StartGame() // Spawn players, etc
        {
            if (!NetworkServer.active) networkManager.StartHost();

            if (isServer)
            {
                print("is server; initializing players");
                InitializePlayers();
            }
        }


        #region CharacterSelect Screen


        public void ShowCharacterSelectScreen()
        {
            if (hasCharacterSelectScreenShown) return;

            if (NetworkServer.active)
            {
                characterSelectScreenContainer.SetActive(true);
            }

            selectMinisButton.onClick.AddListener(() =>
            {

            });

            readyButton.onClick.AddListener(() =>
            {
                characterSelectScreenContainer.SetActive(false);
                StartGame();
            });

            characterSelectScreenContainer.SetActive(true);

            hasCharacterSelectScreenShown = true;
        }
        
        #endregion
        
        [Server]
        private void InitializePlayers()
        {

            foreach (NetworkConnectionToClient player in NetworkServer.connections.Values)
            {
                if (playerInputToNetworkPlayerMap.Keys.Contains(player.connectionId)) continue;

                playerInputToNetworkPlayerMap.Add(
                    player.connectionId,
                    Instantiate(playerPrefab).GetComponent<PlayerCharacterInputController>()
                );
            }
            
            if (!playerInputToNetworkPlayerMap.Keys.Contains(0))
            {
                playerInputToNetworkPlayerMap.Add(
                    0,
                    Instantiate(playerPrefab).GetComponent<PlayerCharacterInputController>()
                );
            }
        }
    }
}
