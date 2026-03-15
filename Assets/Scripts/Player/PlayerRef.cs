using UnityEngine;
using DigDig2.Game;
using UnityEngine.InputSystem;

namespace DigDig2.Player
{
    public class PlayerRef
    {
        public GameObject characterObject;
        public CharacterType characterType;
        public int inputPlayerIndex;
        public InputDevice InputDevice;
    }
}