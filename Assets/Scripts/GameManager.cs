using System.Linq;
using Mirror;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace DigDig2
{

    // HANDELS STUFF SPECIFIC TO GAME SCENE/SEQUENCE
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] GameObject debugStartGameContainer;


        OurNetworkManager networkManager;

        // Flags
        bool hasCharacterSelectScreenShown = false;

        void Start()
        {
            networkManager = OurNetworkManager.instance;

            if (NetworkServer.active)
            {
                Destroy(debugStartGameContainer);
            }
        }

        // Called by the DebugStart Game Screen
        void DebugHostGame()
        {

        }

        // Called by the DebugStart Game Screen
        void DebugTryJoinGame()
        {

        }
        
        public void StartGame() // Spawn players, etc
        {
            if (!NetworkServer.active) networkManager.StartHost();

            if (isServer)
            {
                print("is server, initializing players");
                OurNetworkManager.instance.InitializePlayers(new Vector3(0, 5, 0));
            }
        }
        

        public void ShowCharacterSelectScreen()
        {
            if (hasCharacterSelectScreenShown) return;
            
            // TODO: Implement shit

            hasCharacterSelectScreenShown = true;
        }
        
        

    }
}
