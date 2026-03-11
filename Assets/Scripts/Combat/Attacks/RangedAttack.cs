using Unity.Mathematics;

using UnityEngine;

namespace DigDig2.Combat.Attacks {
	[CreateAssetMenu( fileName = "RangedAttack", menuName = "Scriptable Objects/Attacks/Ranged Attack" )]
	public class RangedAttack : Attack {
		[SerializeField] private string animationStateName;
		[SerializeField] private int damage = 1;
		[SerializeField] private GameObject projectilePrefab;
		[SerializeField] private float projectileSpeed;
		[SerializeField] private float projectileLifetime;

		public override void ChargeStart( Attacker attacker, AttackType attackType ) { }

		public override void Charge( Attacker attacker, AttackType attackType, float chargeTime ) { }

		public override void ChargeFull( Attacker attacker, AttackType attackType ) { }

		public override void Trigger( Attacker attacker, AttackType attackGroup, float chargeTime ) {
			Debug.Log( "Hello i am a ranged attack" );
			attacker.PlayAnimation( animationStateName );
			attacker.AddMoveSpeedDebuff( animationStateName, attacker.GetBaseMoveSpeed( ) / 2 );
			Vector3 forward = attacker.GetComponent<EntityCharacterController>( ).GetForwardVector( );
			Projectile projectile = Instantiate( projectilePrefab, attacker.transform.position + forward, quaternion.LookRotation( forward, Vector3.up ) ).GetComponent<Projectile>( );
			projectile.SetInfo( this, attacker, projectileSpeed, projectileLifetime );
		}

		public override void Ended( Attacker attacker, AttackType attackGroup ) { attacker.RemoveMoveSpeedDebuff( animationStateName ); }

		public override void Hit( Attacker attacker, Attackable attackable, Health healthComponent, EntityCharacterController entityCharacterController ) {
			if ( healthComponent ) healthComponent.Damage( damage );
		}
	}
}
