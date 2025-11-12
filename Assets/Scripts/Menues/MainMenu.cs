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
        [SerializeField] Canvas mainMenuCanvas;
        [SerializeField] GameObject mainMenuContainer;
        [SerializeField] GameObject multiplayerLobbyPrefab;


        public void StartHost()
        {
            mainMenuContainer.SetActive(false);
            Instantiate(multiplayerLobbyPrefab, mainMenuCanvas.transform);
            Debug.Log("Spawned multiplayerlobby, Starting Host...");
            OurNetworkManager.singleton.StartHost();
            Debug.Log("Host started.");
        }

        public void StartJoin()
        {

            mainMenuContainer.SetActive(false);
            Instantiate(multiplayerLobbyPrefab, mainMenuCanvas.transform);
            Debug.Log("Spawned multiplayerlobby, Starting Client...");
            OurNetworkManager.singleton.StartClient();
            Debug.Log("Client started.");
        }
        
        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            Application.Quit();
        }
    }
}


