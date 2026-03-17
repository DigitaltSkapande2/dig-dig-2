using System;
using DigDig2.Combat;
using DigDig2.Debugging;
using DigDig2.Game;
using DigDig2.Input;

using UnityEngine;

namespace DigDig2.Player
{
	[RequireComponent( typeof( Attacker ) )]
	public class SingleplayerCharacterSwitcher : MonoBehaviour
	{
		[SerializeField] private GameObject otherPrefab;
		[SerializeField] private float cooldown;

		private static float lastTimeSwitched;

        public void OnInputGameSwitchCharacter( InputInfo inputInfo )
        {
            Debug.Log($"{name} phaseeee: {inputInfo.context.phase}, ind: {inputInfo.inputPlayerIndex}, device: {inputInfo.context.control.device.name}");
            if (inputInfo.context.started && Time.time - lastTimeSwitched > cooldown &&
                !GameManager.Instance.IsMultiplayer) Switch();
        }

        private void Switch()
        {
            GameManager.Instance.SingleplayerSwitchCharacter( );
            BetterDebug.Log("Switching character :>");
            // foreach (InputModule input in GetComponents<InputModule>())
            // {
            //     input.enabled = false;
            // }
        }
	}
}
