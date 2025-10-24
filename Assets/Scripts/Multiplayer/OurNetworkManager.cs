using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace DigDig2
{
    // Do NOT handle stuff specific to game scene
    public class OurNetworkManager : NetworkManager
    {
        public static OurNetworkManager instance;
        [SerializeField] bool showDebugClinetList = false;
        [SerializeField] public bool isSinglePlayer = true;
        private PlayerCharacterInputController[] playerControllers = new PlayerCharacterInputController[2];

        public UnityEvent onClientListUpdated = new UnityEvent();

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

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            Debug.Log("NETWORKING - Client Connected: " + conn.connectionId);
            if (NetworkServer.connections.Count > 1)
            {
                isSinglePlayer = false;
            }

            base.OnServerConnect(conn);
        }

        public void StartSinglePlayer()
        {
            StartHost(); // TODO: fix singleplayer xD
        }


        #endregion




        [Server]
        public void InitializePlayers(Vector3 Position)
        {
            if (NetworkServer.connections.Values.Count > 2)
            {
                Debug.LogError("More than 2 Connected Clients, This is NOT INTENDED");
                return;
            }
            
            foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
            {
                GameObject player = Instantiate(playerPrefab, Position, Quaternion.identity);

                NetworkServer.AddPlayerForConnection(conn, player);
            }
        }

        public void ToggleSinglePlayerFocus()
        {

        }

        public PlayerCharacterInputController GetLocalPlayerController()
        {
            return playerControllers[0];
        }
    }
}
