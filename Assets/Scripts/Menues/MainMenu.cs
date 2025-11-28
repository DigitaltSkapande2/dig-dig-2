using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DigDig2
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] GameObject LoadingScreenPrefab;
        [Header("Scene References")]
        [SerializeField] Canvas mainMenuCanvas;
        [SerializeField] GameObject mainMenuContainer;
        [SerializeField] string multiplayerLobbySceneName;

        [SerializeField] GameObject multiplayerLobby;


        public async void StartHost()
        {
            mainMenuContainer.SetActive(false);
            Debug.Log("Starting Host...");

            var netManager = OurNetworkManager.singleton;
            netManager.StartHost();

            await UniTask.WaitUntil(() => NetworkServer.active);

            multiplayerLobby.SetActive(true);
            
            Debug.Log("StartHost complete!!");
        }

		public async void StartJoin()
        {

            mainMenuContainer.SetActive(false);
            OurNetworkManager.singleton.StartClient();

            await UniTask.WaitUntil(() => NetworkClient.active);

            multiplayerLobby.SetActive(true);
            
            Debug.Log("Client started.");
        }
        
        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            Application.Quit();
        }
    }
}


