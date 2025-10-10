using System.Linq;
using Mirror;
using NUnit.Framework;
using UnityEngine;

namespace DigDig2
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] public bool isSinglePlayer = false;

        [SerializeField] private GameObject playerPrefab;
        private PlayerCharacterInputController[] playerControllers = new PlayerCharacterInputController[2];
        private int singleplayerCharacterFocus = 0;

        void Start()
        {
            
        }

        [Server]
        private void InitializePlayers()
        {
            
            for (int i = 0; i < playerControllers.Count(); i++)
            {
                playerControllers[i] = Instantiate(playerPrefab).GetComponent<PlayerCharacterInputController>();
            }

            playerControllers[singleplayerCharacterFocus].isSinglePlayerFocus = true;
        }

        public void ToggleSinglePlayerFocus()
        {
            singleplayerCharacterFocus = (singleplayerCharacterFocus + 1) % playerControllers.Count() - 1;
        }



        public void GetSinglePlayerCharacters()
        {

        }
        
        public void GetFocusedSinglePlayerCharacters()
        {
            
        }
        

    }
}
