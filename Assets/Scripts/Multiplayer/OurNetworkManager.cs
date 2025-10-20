using Mirror;
using UnityEngine;

namespace DigDig2
{
    public class OurNetworkManager : NetworkManager
    {
        public static OurNetworkManager instance;
        [SerializeField] public bool isSinglePlayer = false;
        private PlayerCharacterInputController[] playerControllers = new PlayerCharacterInputController[2];

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
            
        }

        #endregion


        [Server]
        private void InitializePlayers(Vector3 Position)
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
    }
}
