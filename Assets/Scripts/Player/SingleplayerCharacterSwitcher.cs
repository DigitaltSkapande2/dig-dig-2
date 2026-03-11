using DigDig2.Combat;
using DigDig2.Game;

using UnityEngine;

namespace DigDig2.Player {
	[RequireComponent( typeof( Attacker ) )]
	public class SingleplayerCharacterSwitcher : MonoBehaviour {
		[SerializeField] private GameObject otherPrefab;
		[SerializeField] private float cooldown;

		private float lastTimeSwitched;

		public void SwitchCharacter( ) {
			if ( Time.time - lastTimeSwitched > cooldown ) GameManager.Instance.SingleplayerSwitchCharacter( );
		}
	}
}
