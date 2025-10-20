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
        private Dictionary<int, CharacterType> clientCharacterTypeSellection = new();

        private void Start()
        {
            startGameButton.interactable = isServer;
        }


        [Client]
        public void ClientSellectCharacter(CharacterType characterType)
        {
            ServerSetClientCharacterSelection(characterType);
        }

        [Command]
        private void ServerSetClientCharacterSelection(CharacterType characterType)
        {
            clientCharacterTypeSellection.Add(clientCharacterTypeSellection);
        }


    }
}