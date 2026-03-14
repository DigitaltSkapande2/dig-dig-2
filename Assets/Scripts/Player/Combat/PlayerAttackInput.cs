using DigDig2.Combat;
using DigDig2.Game;
using DigDig2.Input;

using UnityEngine;

namespace DigDig2.Player.Combat
{
	[RequireComponent( typeof( Attacker ) )]
	public class PlayerAttackInput : MonoBehaviour
	{
		private Attacker attacker;

		private void Awake( ) { attacker = GetComponent<Attacker>( ); }

		#region Input Action Callbacks

		private void OnInputCombatAttack1( InputInfo inputInfo )
		{
			if ( GameManager.Instance.Paused ) return;
			
			if ( inputInfo.context.performed )
				attacker.RequestAttackStart( 0 );
			else
				attacker.RequestAttackEnd( );
		}

		private void OnInputCombatAttack2( InputInfo inputInfo )
		{
			if ( GameManager.Instance.Paused ) return;
			
			if ( inputInfo.context.performed )
				attacker.RequestAttackStart( 1 );
			else
				attacker.RequestAttackEnd( );
		}

		private void OnInputCombatFocus( InputInfo inputInfo )
		{
			if ( inputInfo.context.performed && !GameManager.Instance.Paused )
				attacker.StartFocus( );
			else
				attacker.EndFocus( );
		}

		private void OnInputCombatFocusTarget( InputInfo inputInfo ) { }

		#endregion
	}
}
