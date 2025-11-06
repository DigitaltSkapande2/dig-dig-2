using Mirror;
using TMPro;
using Unity.AppUI.UI;
using UnityEngine;

namespace DigDig2
{
    public class MultiplayerLobby : MonoBehaviour
    {
        [SerializeField] int targetPlayerCount = 2;
        [SerializeField] Button lobbyUIContainer;
        [SerializeField] TMP_Text lobbyPlayerListText;

        OurNetworkManager networkManager;
        void Start()
        {
            networkManager = OurNetworkManager.instance;
            networkManager.onClientListUpdated.AddListener(OnClientListUpdated);
        }

        void OnClientListUpdated()
        {
            if (!NetworkServer.active) return;

            
        }
        



    }
}