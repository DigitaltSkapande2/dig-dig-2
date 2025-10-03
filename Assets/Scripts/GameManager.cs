using System.Linq;
using Mirror;
using UnityEngine;

namespace DigDig2
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] public bool isSinglePlayer = false;

        [SerializeField] private GameObject playerPrefab;
        private PlayerCharacterInputController[] singleplayerPlayerControllers = new PlayerCharacterInputController[2];
        private int singleplayerCharacterFocus = 0;

        void Start()
        {
            
        }

        [Server]
        private void StartSinglePlayer()
        {
            isSinglePlayer = true;

            for (int i = 0; i < singleplayerPlayerControllers.Count(); i++)
            {
                singleplayerPlayerControllers[i] = Instantiate(playerPrefab).GetComponent<PlayerCharacterInputController>();
            }

            singleplayerPlayerControllers[singleplayerCharacterFocus].isSinglePlayerFocus = true;
        }

        public void ToggleSinglePlayerFocus()
        {
            singleplayerCharacterFocus = (singleplayerCharacterFocus + 1) % singleplayerPlayerControllers.Count() - 1;
        }



        public void GetSinglePlayerCharacters()
        {

        }
        
        public void GetFocusedSinglePlayerCharacters()
        {
            
        }
        

    }
}
