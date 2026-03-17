using System;
using UnityEngine;
using DigDig2.Game;
using UnityEngine.InputSystem;

namespace DigDig2.Player
{
    [Serializable]
    public class PlayerRef
    {
        public GameObject characterObject;
        public CharacterType characterType;
        public int inputPlayerIndex = -1;
        public InputDevice InputDevice;
    }
}