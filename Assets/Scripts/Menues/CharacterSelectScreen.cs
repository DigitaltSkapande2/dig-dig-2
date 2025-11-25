using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System.Collections.Generic;


namespace DigDig2
{
    [System.Serializable]
    public enum CharacterType
    {
        Minis,
        Max
    }

    public class CharacterSelectScreen : NetworkBehaviour
    {
        [SerializeField] Button startGameButton;
        [SerializeField] CharacterType characterType;
        private Dictionary<int, CharacterType> clientCharacterTypeSellection = new();

        private void Start()
        {
            startGameButton.interactable = isServer;
            startGameButton.onClick.AddListener(() =>
            {
                Destroy(gameObject);
                GameManager.Instance.StartGame();
            });
        }


        public void ClientSelectCharacter(CharacterType characterType)
        {
            ServerSetClientCharacterSelection(characterType);
        }

        [Command]
        private void ServerSetClientCharacterSelection(CharacterType characterType)
        {
            clientCharacterTypeSellection.Add(connectionToClient.connectionId, characterType);
        }

        
    }
}