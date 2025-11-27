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
        [SerializeField] GameObject multiplayerLobbyPrefab;
        [SerializeField] string multiplayerLobbySceneName;


        public async void StartHost()
        {
            mainMenuContainer.SetActive(false);
            Debug.Log("Starting Host...");

            var netManager = OurNetworkManager.singleton;
            netManager.StartHost();

            await UniTask.WaitUntil(() => NetworkServer.active);

            await SceneManager.LoadSceneAsync(multiplayerLobbySceneName);
            Debug.Log("StartHost complete!!");
        }

        public void StartJoin()
        {

            mainMenuContainer.SetActive(false);
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


