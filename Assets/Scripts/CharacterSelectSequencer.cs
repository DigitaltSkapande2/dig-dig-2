using System;
using System.Linq;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace DigDig2
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class CharacterSelectSequencer : NetworkBehaviour
    {
        #region Variables
        [Header("Prefabs")]
        [SerializeField] private GameObject maxPrefab;
        [SerializeField] private GameObject minisPrefab;
        [Header("Scene Refs")]
        [SerializeField] private Transform maxSpawnPoint;
        [SerializeField] private Transform minisSpawnPoint;
        [Header("UI Refs")]
        [SerializeField] private Button switchCharacterButton;
        [SerializeField] private Button startButton;
        [SerializeField] private TMP_Text maxNameplateText;
        [SerializeField] private TMP_Text minisNameplateText;
        [Header("Debug")]
        [SerializeField] bool verboseLogging = false;


        private bool hostIsMax = true;

        private LocalConnectionToClient hostConn = null;
        private string hostAlias;
        private NetworkConnectionToClient clientConn = null;
        private string clientAlias;

        private GameObject maxInstance;
        private GameObject minisInstance;
        

        #endregion
        #region UnityCallbacks

        private void Awake()
        {
            if (!NetworkManager.singleton.IsMultiplayer)
            {
                VerboseLog("Not in multiplayer mode, disabling character select sequencer.");
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // maxClickableCollider.clickStart.AddListener(() => OnCharacterClicked(CharacterType.Max, maxClickableCollider));
            // minisClickableCollider.clickStart.AddListener(() => OnCharacterClicked(CharacterType.Mini, minisClickableCollider));

            if (!isServer)
            {
                switchCharacterButton.interactable = false;
                startButton.interactable = false;   
            }
            else
            {

                
            }
        }

        #endregion
        #region Server

        public override void OnStartServer()
        {
            VerboseLog("START ON SERVER");
            maxInstance = Instantiate(maxPrefab, maxSpawnPoint.position, maxSpawnPoint.rotation);
            NetworkServer.Spawn(maxInstance);
            minisInstance = Instantiate(minisPrefab, minisSpawnPoint.position, minisSpawnPoint.rotation);
            NetworkServer.Spawn(minisInstance);

            switchCharacterButton.interactable = true;
            startButton.interactable = true;

            hostConn = NetworkServer.localConnection;
            hostAlias = hostConn.connectionId.ToString();

            switchCharacterButton.onClick.AddListener(OnSwitchCharacterButtonClicked);
            startButton.onClick.AddListener(OnStartButtonClicked);

            if (NetworkManager.singleton != null)
            {
                NetworkManager.singleton.serverConnect.AddListener(OnServerConnect);
                NetworkManager.singleton.serverDisconnect.AddListener(OnServerDisconnect);
            }

            VerboseLog("Server ready. Waiting for clients to connect before spawning characters.");
        }

        [Server]
        private void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            if (clientConn == conn)
            {
                clientConn = null;
                clientAlias = "";
            }
            ServerUpdateAliases();
        }

        [Server]
        private void OnServerConnect(NetworkConnectionToClient conn)
        {
            if (clientConn == null)
            {
                VerboseLog($"Client connected: {conn.connectionId}");
                clientConn = conn;
                clientAlias = conn.connectionId.ToString();
            }
            else
            {
                Debug.LogError("there are more than 2 connections?");
            }
            ServerUpdateAliases();
        }

        [Server]
        private void OnSwitchCharacterButtonClicked()
        {
            hostIsMax = !hostIsMax;
            RpcUpdateHostIsMax(hostIsMax);

            ServerUpdateAliases();
        }

        [Server]
        private void OnStartButtonClicked()
        {
            GameObject hostCharObjInstance = hostIsMax ? maxInstance : minisInstance;
            GameObject clientCharObjInstance = hostIsMax ? minisInstance : maxInstance;

            if (clientConn == null)
            {
                Debug.LogError("No client connected! Cannot start game.");
                return;
            }

            NetworkServer.ReplacePlayerForConnection(hostConn, hostCharObjInstance, ReplacePlayerOptions.KeepActive);
            NetworkServer.ReplacePlayerForConnection(clientConn, clientCharObjInstance, ReplacePlayerOptions.KeepActive);

            RpcEnablePlayerInput();
        }

        [Server]
        private void ServerUpdateAliases() 
        {
            RpcUpdateAliases(hostAlias, clientAlias);
            //LocalUpdateCharacterNamePlates();
        }

        #endregion
        #region Client

        [ClientRpc]
        private void RpcUpdateHostIsMax(bool newValue)
        {
            hostIsMax = newValue;
            LocalUpdateCharacterNamePlates();
        }

        [ClientRpc]
        private void RpcUpdateAliases(string clientAlias, string hostAlias)
        {
            this.clientAlias = clientAlias;
            this.hostAlias = hostAlias;
            LocalUpdateCharacterNamePlates();
        }

        [ClientRpc]
        private void RpcEnablePlayerInput() 
        {
            NetworkClient.localPlayer.GetComponent<PlayerCharacterInputController>().EnableInput();
            NetworkClient.localPlayer.GetComponent<PlayerAttackInput>().EnableInput();
            NetworkClient.localPlayer.GetComponent<EntityCharacterController>().Frozen = false;
        }

        #endregion
        #region Util

        private void LocalUpdateCharacterNamePlates()
        {
            maxNameplateText.text = hostIsMax ? hostAlias : clientAlias;
            minisNameplateText.text = hostIsMax ? clientAlias : hostAlias; 
        }

        private void VerboseLog(string msg)
        {
            if (verboseLogging) Debug.Log("CHARACTER_SELECT: " + msg);
        }
        
        #endregion
    }
}
