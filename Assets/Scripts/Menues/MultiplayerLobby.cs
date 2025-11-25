using System.Linq;
using Mirror;
using TMPro;
using Unity.AppUI.UI;
using UnityEngine;

namespace DigDig2
{
    public class MultiplayerLobby : NetworkBehaviour
    {
        [SerializeField] int targetPlayerCount = 2;
        [SerializeField] TMP_Text lobbyPlayerListText;

        [SyncVar] int[] serverConnectionIDs;

        void Start()
        {
            if (NetworkServer.active)
            {
                Die();
            }

            if (isServer) OurNetworkManager.singleton.OnServerConnectAction += OnServerConnect;
        }

        void Update()
        {
            UpdateConnectionsListText();
        }

        [Server]
        public void OnServerConnect(NetworkConnectionToClient conn)
        {
            serverConnectionIDs = NetworkServer.connections.Keys.ToArray();
        }

        void UpdateConnectionsListText()
        {
            string gay = "";
            foreach (int clID in serverConnectionIDs)
            {
                gay += $"{clID.ToString()}\n";
            }
            lobbyPlayerListText.text = gay;
        }


        private void Die()
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

    }
}