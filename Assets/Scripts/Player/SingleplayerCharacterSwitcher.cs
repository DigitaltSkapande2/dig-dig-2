using System;
using DigDig2.Combat;
using DigDig2.Game;

using UnityEngine;

namespace DigDig2.Player
{
	[RequireComponent( typeof( Attacker ) )]
	public class SingleplayerCharacterSwitcher : MonoBehaviour
	{
		[SerializeField] private GameObject otherPrefab;
		[SerializeField] private float cooldown;

		private float lastTimeSwitched;

        private void Start()
        {
            if (!GameManager.Instance.IsMultiplayer)
            {
                enabled = false;
                Destroy(this);
            }
        }

        public void SwitchCharacter( )
		{
			if ( Time.time - lastTimeSwitched > cooldown && !GameManager.Instance.IsMultiplayer ) GameManager.Instance.SingleplayerSwitchCharacter( );
		}
	}
}
