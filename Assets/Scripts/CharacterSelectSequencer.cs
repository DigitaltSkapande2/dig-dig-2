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

        private NetworkConnectionToClient hostConn = null;
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
                switchCharacterButton.enabled = false;
                startButton.enabled = false;
            }
        }

        #endregion
        #region Server

        public override void OnStartServer()
        {
            VerboseLog("START ON SERVER");
            maxInstance = Instantiate(maxPrefab, maxSpawnPoint.position, maxSpawnPoint.rotation);
            minisInstance = Instantiate(minisPrefab, minisSpawnPoint.position, minisSpawnPoint.rotation);

            switchCharacterButton.enabled = true;
            startButton.enabled = true;

            hostConn = NetworkClient.localPlayer.connectionToClient;

            switchCharacterButton.onClick.AddListener(OnSwitchCharacterButtonClicked);
            startButton.onClick.AddListener(OnStartButtonClicked);

            if (NetworkManager.singleton != null)
            {
                NetworkManager.singleton.serverConnect.RemoveListener(OnServerConnect);
                NetworkManager.singleton.serverDisconnect.RemoveListener(OnServerDisconnect);
            }
        }

        [Server]
        private void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            if (clientConn == conn)
            {
                clientConn = null;
                clientAlias = "";
            }
            UpdateCharacterNamePlates();
        }

        [Server]
        private void OnServerConnect(NetworkConnectionToClient conn)
        {
            if (clientConn == null)
            {
                VerboseLog($"Set Max Connection to {conn.connectionId}");
                clientConn = conn;
                clientAlias = conn.connectionId.ToString();
            }
            else
            {
                Debug.LogError("there are more than 2 connections?");
            }
            UpdateCharacterNamePlates();
        }

        [Server]
        private void OnSwitchCharacterButtonClicked()
        {
            hostIsMax = !hostIsMax;

            UpdateCharacterNamePlates();
        }

        [Server]
        private void OnStartButtonClicked()
        {
            //NetworkServer.ReplacePlayerForConnection() // TODOreplace characters,
            GameObject hostCharObjInstance = hostIsMax ? maxInstance : minisInstance;
            GameObject clientCharObjInstance = hostIsMax ? minisInstance : maxInstance;

            NetworkServer.ReplacePlayerForConnection(hostConn, hostCharObjInstance, ReplacePlayerOptions.KeepActive);
            NetworkServer.ReplacePlayerForConnection(clientConn, clientCharObjInstance, ReplacePlayerOptions.KeepActive);
        }

        [Server]
        private void OnCharacterClicked(CharacterType characterType, ClickableMesh clickedMesh)
        {
            Debug.Log("Clicked on character: " + characterType);
        }

        [Server]
        private void FixedUpdate()
        {

        }

        #endregion
        #region Client

        [Client, ClientRpc]
        private void RpcUpdateHostIsMax(bool newValue)
        {
            hostIsMax = newValue;
        }

        [Client, ClientRpc]
        private void RpcUpdateAliases(string clientAlias, string hostAlias)
        {
            
        }


        #endregion
        #region Util


        private void UpdateCharacterNamePlates(bool oldValue = false, bool newValue = false)
        {
            maxNameplateText.text = hostIsMax ? hostAlias : clientAlias;
            minisNameplateText.text = hostIsMax ? clientAlias : hostAlias; ;
        }

        private void VerboseLog(string msg)
        {
            if (verboseLogging) Debug.Log("CHARACTER_SELECT: " + msg);
        }
        
        #endregion
    }
}
