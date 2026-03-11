using UnityEngine;

namespace DigDig2.Combat.Attacks
{
	[CreateAssetMenu( fileName = "MeleeAttack", menuName = "Scriptable Objects/Attacks/Melee Attack" )]
	public class MeleeAttack : Attack
	{
		[SerializeField] private string animationStateName;
		[SerializeField] private int damage = 1;
		[SerializeField] private int bindableAttackHitboxIndex;
		[SerializeField] private GameObject hitEffect;
		[SerializeField] private bool hasTrailEffect;
		[SerializeField] private float knockbackStrength = 50;

		public override void ChargeStart( Attacker attacker, AttackType attackType ) { }

		public override void Charge( Attacker attacker, AttackType attackType, float chargeTime ) { }

		public override void ChargeFull( Attacker attacker, AttackType attackType ) { }

		public override void Trigger( Attacker attacker, AttackType attackGroup, float chargeTime )
		{
			if ( hasTrailEffect ) attacker.GetattackTrailRenderer( ).enabled = true;
			attacker.PlayAnimation( animationStateName );
			attacker.StartHitboxAttack( this, animationStateName, attacker.GetBindableAttackHitbox( bindableAttackHitboxIndex ) );
			attacker.AddMoveSpeedDebuff( animationStateName, attacker.GetBaseMoveSpeed( ) );
			attacker.PushInDirection( Vector3.forward, 10 );
		}

		public override void Ended( Attacker attacker, AttackType attackGroup )
		{
			if ( hasTrailEffect ) attacker.GetattackTrailRenderer( ).enabled = false;
			attacker.EndHitboxAttack( animationStateName );
			attacker.RemoveMoveSpeedDebuff( animationStateName );
		}

		public override void Hit( Attacker attacker, Attackable attackable, Health healthComponent, EntityCharacterController entityCharacterController )
		{
			if ( hitEffect ) Instantiate( hitEffect, attackable.transform.position, Quaternion.identity );
			if ( healthComponent ) healthComponent.Damage( damage );
			attackable.ApplyKnockback( ( attackable.transform.position - attacker.transform.position ).normalized, knockbackStrength );
		}
	}
}
