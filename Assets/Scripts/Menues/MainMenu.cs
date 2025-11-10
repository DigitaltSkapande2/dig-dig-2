using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Mirror;
using TMPro;
using UnityEngine;

namespace DigDig2
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] GameObject LoadingScreenPrefab;
        [Header("Scene References")]
        [SerializeField] GameObject mainMenuContainer;
        [SerializeField] Canvas multiplayerLobbyPrefab;


        public void StartHost()
        {
            OurNetworkManager.singleton.StartHost();
            mainMenuContainer.SetActive(false);
            Instantiate(multiplayerLobbyPrefab);    
        }

        public void StartJoin()
        {
            OurNetworkManager.singleton.StartClient();
            mainMenuContainer.SetActive(false);
            Instantiate(multiplayerLobbyPrefab);
        }
        
    

    }
}


