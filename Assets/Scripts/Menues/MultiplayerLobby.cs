using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DigDig2
{   
    public class MultiplayerLobby : NetworkBehaviour
    {
        [SerializeField] private TMP_Text maxPlayerNameText;
        [SerializeField] private TMP_Text miniPlayerNameText;

        [SerializeField] private Button startButton;
        [SerializeField] private Button switchButton;

        private NetworkConnectionToClient maxPlayerConnection;
        [SerializeField, SyncVar(hook = nameof(UpdatePlayerCharacters))] private string maxPlayerName = "";
        private NetworkConnectionToClient miniPlayerConnection;
        [SerializeField, SyncVar(hook = nameof(UpdatePlayerCharacters))] private string miniPlayerName = "";



        public override void OnStartServer()
        {
            if (NetworkManager.singleton != null)
            {
                NetworkManager.singleton.serverConnect.AddListener(OnServerConnect);
                NetworkManager.singleton.serverDisconnect.AddListener(OnServerDisconnect);
            }

            if (!NetworkServer.active) return;

            if (isServer)
            {
                switchButton.interactable = true;
                maxPlayerConnection = NetworkClient.localPlayer.connectionToClient;
                UpdatePlayers();
            }
            else
            {
                switchButton.interactable = false;
                UpdatePlayerCharacters();
            }
        }

        private void OnDestroy()
        {
            if (isServer)
            {
                if (NetworkManager.singleton != null)
                {
                    NetworkManager.singleton.serverConnect.RemoveListener(OnServerConnect);
                    NetworkManager.singleton.serverDisconnect.RemoveListener(OnServerDisconnect);
                }
            }

        }
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            UpdatePlayerCharacters();
        }

        [Server]
        private void OnServerConnect(NetworkConnectionToClient conn)
        {
            if (maxPlayerConnection == null)
            {
                maxPlayerConnection = conn;
                maxPlayerName = conn.connectionId.ToString();
            }
            else if (miniPlayerConnection == null)
            {
                miniPlayerConnection = conn;
                miniPlayerName = conn.connectionId.ToString();
            }
            else
            {
                Debug.LogError("Could not assign a character to new connection because there was no assignable character, are there more than 2 connections?");
            }

            UpdatePlayers();
        }
        [Server]
        private void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            if (maxPlayerConnection == conn)
            {
                maxPlayerConnection = null;
                maxPlayerName = "";
            }
            else if (miniPlayerConnection == conn)
            {
                miniPlayerConnection = null;
                miniPlayerName = "";
            }
            UpdatePlayers();
        }
        [Server]
        private void UpdatePlayers()
        {
            maxPlayerName = maxPlayerConnection != null ? maxPlayerConnection.connectionId.ToString() : "";
            miniPlayerName = miniPlayerConnection != null ? miniPlayerConnection.connectionId.ToString() : "";

            startButton.interactable = maxPlayerConnection != null && miniPlayerConnection != null;

            UpdatePlayerCharacters();
        }

        private void UpdatePlayerCharacters(string oldValue = "", string newValue = "")
        {
            maxPlayerNameText.text = maxPlayerName != "" ? maxPlayerName : "Waiting for Player...";
            miniPlayerNameText.text = miniPlayerName != "" ? miniPlayerName : "Waiting for Player...";
        }

        public void Disconnect()
        {
            if (isServer)
            {
                NetworkManager.singleton.StopServer();
            }

            if (isClient)
            {
                NetworkManager.singleton.StopClient();
            }
        }

        [Server]
        public void SwitchCharacters()
        {
            NetworkConnectionToClient preSwitchMax = maxPlayerConnection;
            NetworkConnectionToClient preSwitchMini = miniPlayerConnection;

            maxPlayerConnection = preSwitchMini;
            miniPlayerConnection = preSwitchMax;

            UpdatePlayers();
        }

        [Server]
        public void StartLobby()
        {
            NetworkManager.singleton.SetCharacters(maxPlayerConnection, miniPlayerConnection);
            NetworkManager.singleton.ServerChangeScene("THEGameh");
        }
    }
}