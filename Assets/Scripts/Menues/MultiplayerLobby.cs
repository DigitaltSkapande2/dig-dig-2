using System.Text;
using Mirror;
using TMPro;
using UnityEngine;

namespace DigDig2
{
    public class MultiplayerLobby : NetworkBehaviour
    {
        [SerializeField] int targetPlayerCount = 2;
        [SerializeField] TMP_Text lobbyPlayerListText;

        [SerializeField] bool verboseLogging = false;

        // Use a SyncList so the server can modify the list and it will be synced to clients.
        public readonly SyncList<int> serverConnectionIDs = new SyncList<int>();

#region Server-side Init


        private void Start()
        {
            if (OurNetworkManager.singleton != null)
            {
                OurNetworkManager.singleton.OnServerConnectAction += OnServerConnect;
                OurNetworkManager.singleton.OnServerDisconnectAction += OnServerDisconnect;
            }

            if (!NetworkServer.active) return;
            if (isServer) UpdateServerConnectionIDs();
            UpdateConnectionsListText();
        }

        private void OnDestroy()
        {
            if (OurNetworkManager.singleton != null)
            {
                OurNetworkManager.singleton.OnServerConnectAction -= OnServerConnect;
                OurNetworkManager.singleton.OnServerDisconnectAction -= OnServerDisconnect;
            }
        }

#endregion
#region Client-side Init
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            Log("OnClient Start");
            // subscribe to list changes so UI updates automatically when the server changes the list
            serverConnectionIDs.Callback += OnServerConnectionIDsChanged;
            UpdateConnectionsListText();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            serverConnectionIDs.Callback -= OnServerConnectionIDsChanged;
        }

#endregion
#region Sync Communication

        [Server]
        void OnServerConnect(NetworkConnectionToClient conn)
        {
            Log("OnServerConnect");
            UpdateServerConnectionIDs();
        }

        [Server]
        void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            Log("OnServerDisconnect");
            UpdateServerConnectionIDs();
        }

        [Server]
        void UpdateServerConnectionIDs()
        {
            Log("OnServer Update Connection IDs");
            serverConnectionIDs.Clear();
            foreach (var kv in NetworkServer.connections)
            {
                serverConnectionIDs.Add(kv.Key);
            }

            UpdateConnectionsListText();
        }

        void OnServerConnectionIDsChanged(SyncList<int>.Operation op, int index, int oldItem, int newItem)
        {
            Log("connections uppdated");
            UpdateConnectionsListText();
        }

#endregion
#region Misc

        void UpdateConnectionsListText()
        {
            if (lobbyPlayerListText == null) return;

            if (serverConnectionIDs == null || serverConnectionIDs.Count == 0)
            {
                lobbyPlayerListText.text = "No players";
                return;
            }

            var sb = new StringBuilder();
            foreach (int clID in serverConnectionIDs)
            {
                sb.AppendLine(clID.ToString());
            }

            lobbyPlayerListText.text = sb.ToString();
        }

        void Log(string msg) 
        {
            Debug.Log(msg);
        }

#endregion
    }
}