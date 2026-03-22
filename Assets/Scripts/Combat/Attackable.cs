using DigDig2.EffectSystem;

using UnityEngine;
using UnityEngine.Events;

namespace DigDig2.Combat
{
	public class Attackable : MonoBehaviour
	{
		[Tooltip( "Duration of invincibility after a hit." )]
		[SerializeField] private float invincibilityTime = 0.05f;

		[Tooltip( "The group that this attackable is in. Used to filter attacks to only damage certain attackables." )]
		[SerializeField] private string group = "none";

		[Tooltip( "Effects that are played when the attackable is hit." )]
		[SerializeField] private EffectPlayer hitEffect;

		[Tooltip( "An event that gets emitted when this attackable gets hit." )]
		[SerializeField] public UnityEvent hit = new( );

		private EntityCharacterController entityCharacterController;

		private Health healthComponent;

		private float invincibilityTimer;

		public string Group
		{
			get => group;
		}

		private void Awake( )
		{
			TryGetComponent( out healthComponent );
			TryGetComponent( out entityCharacterController );
		}

		private void Update( )
		{
			if ( invincibilityTimer > 0 ) invincibilityTimer -= Time.deltaTime;
		}

		public bool Hit( Attack attack, Attacker attacker = null )
        {
            if ( attacker && !attacker.CanAttackGroup( group ) ) return false;
			if ( invincibilityTimer > 0 ) return false;

			invincibilityTimer = invincibilityTime;

			if ( attack ) attack.Hit( attacker, this, healthComponent, entityCharacterController );

			hit.Invoke( );
			hitEffect?.Play( transform.position );

            return true;
        }

		public bool IsInvincible( ) => invincibilityTimer > 0;

		public void ApplyKnockback( Vector3 direction, float strength )
		{
			if ( entityCharacterController ) entityCharacterController.ApplyKnockback( direction, strength );
		}
	}
}
