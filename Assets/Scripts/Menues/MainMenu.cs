using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace DigDig2
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] GameObject LoadingScreenPrefab;
        [Header("Scene References")]
        [SerializeField] GameObject mainMenuContainer;
        [SerializeField] GameObject multiplayerLobbyPanel;

        public void StartHost()
        {
            OurNetworkManager.instance.StartHost();
            mainMenuContainer.SetActive(false);
            multiplayerLobbyPanel.SetActive(true);
        }
        
        public void StartJoin()
        {
            OurNetworkManager.instance.StartClient();
        }

    }
}


