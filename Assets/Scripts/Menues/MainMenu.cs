using Cysharp.Threading.Tasks;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DigDig2
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] GameObject mainMenuContainer;
        [SerializeField] GameObject multiplayerLobbyContainer;



        private void Start()
        {
            NetworkManager.singleton.clientDisconnect.AddListener(() =>
            {
                multiplayerLobbyContainer.SetActive(false);
                mainMenuContainer.SetActive(true);
            });
        }

        public async void StartHost()
        {
            mainMenuContainer.SetActive(false);

            NetworkManager.singleton.StartHost();
            await UniTask.WaitUntil(() => NetworkServer.active);

            multiplayerLobbyContainer.SetActive(true);
        }

        public async void StartJoin()
        {

            mainMenuContainer.SetActive(false);

            NetworkManager.singleton.StartClient();
            await UniTask.WaitUntil(() => NetworkClient.active);

            multiplayerLobbyContainer.SetActive(true);

            Debug.Log("Client started.");
        }
        
        public async void StartSingleplayer()
        {
            NetworkManager.singleton.StartHost(false);

            await UniTask.WaitUntil(() => NetworkServer.active);

            NetworkManager.singleton.ServerChangeScene(SceneManager.GetSceneByBuildIndex(1).name);
        }
        
        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            Application.Quit();
        }
    }
}


