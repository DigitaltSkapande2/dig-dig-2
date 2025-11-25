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
        [Header("Debug")]
        [SerializeField] bool verboseLogging;

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
            networkManager.StartHost();
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

            selectMaxButton.onClick.AddListener(() =>
            {
                
            });

            selectMinisButton.onClick.AddListener(() =>
            {
                
            });

            readyButton.onClick.AddListener(() =>
            {
                if (isServer) return;
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
            Log("--- InitializingPlayerControllers ---");
            foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
            {
                Log($"Spawning player for conn: {conn.connectionId}");
                if (playerInputToNetworkPlayerMap.Keys.Contains(conn.connectionId)) continue;

                var playerInstance = Instantiate(playerPrefab);

                playerInputToNetworkPlayerMap.Add(
                    conn.connectionId,
                    playerInstance.GetComponent<PlayerCharacterInputController>()
                );

                playerInstance.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
                NetworkServer.AddPlayerForConnection(conn, playerInstance);
            }

            if (!playerInputToNetworkPlayerMap.Keys.Contains(0))
            {
                var playerInstance = Instantiate(playerPrefab);

                playerInputToNetworkPlayerMap.Add(
                    0,
                    playerInstance.GetComponent<PlayerCharacterInputController>()
                );

                playerInstance.name = $"{playerPrefab.name} [connId={NetworkServer.localConnection.connectionId}]";
                NetworkServer.AddPlayerForConnection(NetworkServer.localConnection, playerInstance);
            }
        }
        

        private void Log(string msg)
        {
            if (verboseLogging) Debug.Log(msg);
        } 
    }
}
