// using System;
// using UnityEngine;
// using UnityEngine.Events;
// 
// using kcp2k;

// /*
// 	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
// 	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
// */
// namespace DigDig2
// {
//     public class NetworkManager : Mirror.NetworkManager
//     {
//         public bool IsMultiplayer { get; private set; } = false;

//         [SerializeField] private Canvas debugCanvas;

//         // You can adjust the parameters of the Actions below to suit your needs and pass the values through the Invoke() method.
//         [NonSerialized] public UnityEvent onDestroy = new();

//         [NonSerialized] public UnityEvent<string> serverChangeScene = new();
//         [NonSerialized] public UnityEvent<string> serverSceneChanged = new();
//         [NonSerialized] public UnityEvent<string, SceneOperation, bool> clientChangeScene = new();
//         [NonSerialized] public UnityEvent clientSceneChanged = new();

//         [NonSerialized] public UnityEvent<NetworkConnectionToClient> serverConnect = new();
//         [NonSerialized] public UnityEvent<NetworkConnectionToClient> serverReady = new();
//         [NonSerialized] public UnityEvent<NetworkConnectionToClient> serverAddPlayer = new();
//         [NonSerialized] public UnityEvent<NetworkConnectionToClient> serverDisconnect = new();
//         [NonSerialized] public UnityEvent<NetworkConnectionToClient, TransportError, string> serverError = new();
//         [NonSerialized] public UnityEvent<NetworkConnectionToClient, Exception> serverTransportException = new();

//         [NonSerialized] public UnityEvent clientConnect = new();
//         [NonSerialized] public UnityEvent clientDisconnect = new();
//         [NonSerialized] public UnityEvent clientNotReady = new();
//         [NonSerialized] public UnityEvent<TransportError, string> clientError = new();
//         [NonSerialized] public UnityEvent<Exception> clientTransportException = new();

//         [NonSerialized] public UnityEvent startServer = new();
//         [NonSerialized] public UnityEvent stopServer = new();
//         [NonSerialized] public UnityEvent startHost = new();
//         [NonSerialized] public UnityEvent stopHost = new();
//         [NonSerialized] public UnityEvent startClient = new();
//         [NonSerialized] public UnityEvent stopClient = new();

//         public NetworkConnectionToClient MaxPlayerConnection { get; private set; }
//         public NetworkConnectionToClient MiniPlayerConnection { get; private set; }
//         public bool CharactersSelected { get; private set; } = false;

//         // Overrides the base singleton so we don't have to cast to this type everywhere.
//         public static new NetworkManager singleton => (NetworkManager)Mirror.NetworkManager.singleton;

//         /// <summary>
//         /// Runs on both Server and Client
//         /// Networking is NOT initialized when this fires
//         /// </summary>
//         public override void Awake()
//         {
//             base.Awake();
//         }

//         #region Unity Callbacks

//         public override void OnValidate()
//         {
//             base.OnValidate();
//         }

//         /// <summary>
//         /// Runs on both Server and Client
//         /// Networking is NOT initialized when this fires
//         /// </summary>
//         public override void Start()
//         {
//             base.Start();

//             if (transport.GetType() == typeof(KcpTransport))
//             {
//                 //debugCanvas.gameObject.SetActive(true);
//             }
//         }

//         /// <summary>
//         /// Runs on both Server and Client
//         /// </summary>
//         public override void LateUpdate()
//         {
//             base.LateUpdate();
//         }

//         /// <summary>
//         /// Runs on both Server and Client
//         /// </summary>
//         public override void OnDestroy()
//         {
//             onDestroy?.Invoke();
//             base.OnDestroy();
//         }

//         #endregion

//         #region Start & Stop

//         public void StartHost(bool isMultiplayer = true)
//         {
//             IsMultiplayer = isMultiplayer;
//             base.StartHost();
//             debugCanvas.gameObject.SetActive(false);
//         }

//         public new void StartClient(bool isMultiplayer = true)
//         {
//             IsMultiplayer = isMultiplayer;
//             base.StartClient();
//             debugCanvas.gameObject.SetActive(false);
//         }

//         /// <summary>
//         /// Set the frame rate for a headless server.
//         /// <para>Override if you wish to disable the behavior or set your own tick rate.</para>
//         /// </summary>
//         public override void ConfigureHeadlessFrameRate()
//         {
//             base.ConfigureHeadlessFrameRate();
//         }

//         #endregion

//         #region Scene Management

//         /// <summary>
//         /// This causes the server to switch scenes and sets the networkSceneName.
//         /// <para>Clients that connect to this server will automatically switch to this scene. This is called automatically if onlineScene or offlineScene are set, but it can be called from user code to switch scenes again while the game is in progress. This automatically sets clients to be not-ready. The clients must call NetworkClient.Ready() again to participate in the new scene.</para>
//         /// </summary>
//         /// <param name="newSceneName"></param>
//         public override void ServerChangeScene(string newSceneName)
//         {
//             base.ServerChangeScene(newSceneName);
//         }


//         /// <summary>
//         /// Called from ServerChangeScene immediately before SceneManager.LoadSceneAsync is executed
//         /// <para>This allows server to do work / cleanup / prep before the scene changes.</para>
//         /// </summary>
//         /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
//         public override void OnServerChangeScene(string newSceneName)
//         {
//             serverChangeScene?.Invoke(newSceneName);
//         }

//         /// <summary>
//         /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
//         /// </summary>
//         /// <param name="sceneName">The name of the new scene.</param>
//         public override void OnServerSceneChanged(string sceneName)
//         {
//             serverSceneChanged?.Invoke(sceneName);
//         }

//         /// <summary>
//         /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
//         /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
//         /// </summary>
//         /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
//         /// <param name="sceneOperation">Scene operation that's about to happen</param>
//         /// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
//         public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
//         {
//             clientChangeScene?.Invoke(newSceneName, sceneOperation, customHandling);
//         }

//         /// <summary>
//         /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
//         /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
//         /// </summary>
//         public override void OnClientSceneChanged()
//         {
//             clientSceneChanged?.Invoke();
//             base.OnClientSceneChanged();
//         }

//         #endregion

//         #region Server System Callbacks

//         /// <summary>
//         /// Called on the server when a new client connects.
//         /// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
//         /// </summary>
//         /// <param name="conn">Connection from client.</param>
//         public override void OnServerConnect(NetworkConnectionToClient conn)
//         {
//             serverConnect?.Invoke(conn);
//         }

//         /// <summary>
//         /// Called on the server when a client is ready.
//         /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
//         /// </summary>
//         /// <param name="conn">Connection from client.</param>
//         public override void OnServerReady(NetworkConnectionToClient conn)
//         {
//             serverReady?.Invoke(conn);
//             base.OnServerReady(conn);
//         }

//         /// <summary>
//         /// Called on the server when a client adds a new player with ClientScene.AddPlayer.
//         /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
//         /// </summary>
//         /// <param name="conn">Connection from client.</param>
//         public override void OnServerAddPlayer(NetworkConnectionToClient conn)
//         {
//             serverAddPlayer?.Invoke(conn);
//             base.OnServerAddPlayer(conn);
//         }

//         /// <summary>
//         /// Called on the server when a client disconnects.
//         /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
//         /// </summary>
//         /// <param name="conn">Connection from client.</param>
//         public override void OnServerDisconnect(NetworkConnectionToClient conn)
//         {
//             serverDisconnect?.Invoke(conn);
//             base.OnServerDisconnect(conn);
//         }

//         /// <summary>
//         /// Called on server when transport raises an error.
//         /// <para>NetworkConnection may be null.</para>
//         /// </summary>
//         /// <param name="conn">Connection of the client...may be null</param>
//         /// <param name="transportError">TransportError enum</param>
//         /// <param name="message">String message of the error.</param>
//         public override void OnServerError(NetworkConnectionToClient conn, TransportError transportError, string message)
//         {
//             serverError?.Invoke(conn, transportError, message);
//         }

//         /// <summary>
//         /// Called on server when transport raises an exception.
//         /// <para>NetworkConnection may be null.</para>
//         /// </summary>
//         /// <param name="conn">Connection of the client...may be null</param>
//         /// <param name="exception">Exception thrown from the Transport.</param>
//         public override void OnServerTransportException(NetworkConnectionToClient conn, Exception exception)
//         {
//             serverTransportException?.Invoke(conn, exception);
//         }

//         #endregion

//         #region Client System Callbacks

//         /// <summary>
//         /// Called on the client when connected to a server.
//         /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
//         /// </summary>
//         public override void OnClientConnect()
//         {
//             clientConnect?.Invoke();
//             base.OnClientConnect();
//         }

//         /// <summary>
//         /// Called on clients when disconnected from a server.
//         /// <para>This is called on the client when it disconnects from the server. Override this function to decide what happens when the client disconnects.</para>
//         /// </summary>
//         public override void OnClientDisconnect()
//         {
//             IsMultiplayer = false;
//             clientDisconnect?.Invoke();
//         }

//         /// <summary>
//         /// Called on clients when a servers tells the client it is no longer ready.
//         /// <para>This is commonly used when switching scenes.</para>
//         /// </summary>
//         public override void OnClientNotReady()
//         {
//             clientNotReady?.Invoke();
//         }

//         /// <summary>
//         /// Called on client when transport raises an error.</summary>
//         /// </summary>
//         /// <param name="transportError">TransportError enum.</param>
//         /// <param name="message">String message of the error.</param>
//         public override void OnClientError(TransportError transportError, string message)
//         {
//             clientError?.Invoke(transportError, message);
//         }

//         /// <summary>
//         /// Called on client when transport raises an exception.</summary>
//         /// </summary>
//         /// <param name="exception">Exception thrown from the Transport.</param>
//         public override void OnClientTransportException(Exception exception)
//         {
//             clientTransportException?.Invoke(exception);
//         }

//         #endregion

//         #region Start & Stop Callbacks

//         // Since there are multiple versions of StartServer, StartClient and StartHost, to reliably customize
//         // their functionality, users would need override all the versions. Instead these callbacks are invoked
//         // from all versions, so users only need to implement this one case.

//         /// <summary>
//         /// This is invoked when a server is started - including when a host is started.
//         /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
//         /// </summary>
//         public override void OnStartServer()
//         {
//             startServer?.Invoke();
//         }

//         /// <summary>
//         /// This is called when a server is stopped - including when a host is stopped.
//         /// </summary>
//         public override void OnStopServer()
//         {
//             stopServer?.Invoke();
//         }

//         /// <summary>
//         /// This is invoked when a host is started.
//         /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
//         /// </summary>
//         public override void OnStartHost()
//         {
//             startHost?.Invoke();
//         }

//         /// <summary>
//         /// This is called when a host is stopped.
//         /// </summary>
//         public override void OnStopHost()
//         {
//             IsMultiplayer = false;
//             stopHost?.Invoke();
//         }

//         /// <summary>
//         /// This is invoked when the client is started.
//         /// </summary>
//         public override void OnStartClient()
//         {
//             startClient?.Invoke();
//         }

//         /// <summary>
//         /// This is called when a client is stopped.
//         /// </summary>
//         public override void OnStopClient()
//         {
//             IsMultiplayer = false;
//             stopClient?.Invoke();
//         }

//         #endregion

//         #region Character Selection

//         public void SetCharacters(NetworkConnectionToClient maxPlayerConnection, NetworkConnectionToClient miniPlayerConnection)
//         {
//             if (CharactersSelected) return;
//             CharactersSelected = true;

//             MaxPlayerConnection = maxPlayerConnection;
//             MiniPlayerConnection = miniPlayerConnection;
//         }

//         #endregion
//     }
// }