using UnityEngine;
using DigDig2.Entity;

namespace DigDig2.Combat.Attacks
{
	[CreateAssetMenu( fileName = "MeleeAttack", menuName = "Scriptable Objects/Attacks/Melee Attack" )]
	public class MeleeAttack : Attack
	{
		[SerializeField] private string animationStateName;
		[SerializeField] private int damage = 1;
		[SerializeField] private float pushAmount;
		[SerializeField] private int bindableAttackHitboxIndex;
		[SerializeField] private GameObject hitEffect;
		[SerializeField] private GameObject attackVFX;
		[SerializeField] private float knockbackStrength = 50;

		public override void ChargeStart( Attacker attacker, AttackType attackType ) { }

		public override void Charge( Attacker attacker, AttackType attackType, float chargeTime ) { }

		public override void ChargeFull( Attacker attacker, AttackType attackType ) { }

		public override void Trigger( Attacker attacker, AttackType attackGroup, float chargeTime )
		{
			onPerformEffect?.Play();
			attacker.PlayAnimation( animationStateName );
			attacker.StartHitboxAttack( this, animationStateName, attacker.GetBindableAttackHitbox( bindableAttackHitboxIndex ) );
			attacker.AddMoveSpeedDebuff( animationStateName, attacker.GetBaseMoveSpeed( ) );
		}
		
		public override void AnimationEvent(Attacker attacker, AttackType attackGroup, string animEventName)
        {
			if (animEventName == "Push")
			{
				attacker.PushInDirection( Vector3.forward, pushAmount );
			}

			if (animEventName == "AttackVFX")
			{	
				Quaternion rotation = Quaternion.LookRotation(attacker.GetForwardVector(), attacker.transform.up);
				Destroy(Instantiate(attackVFX, attacker.transform.position, rotation, attacker.transform), 5);
			}

			if (animEventName == "EnemyAttackVFX1")
			{
				Quaternion rotation = Quaternion.LookRotation(attacker.GetForwardVector(), attacker.transform.up + Vector3.Cross(attacker.GetForwardVector(), attacker.transform.up));
				Destroy(Instantiate(attackVFX, attacker.transform.position + attacker.GetForwardVector(), rotation, attacker.transform), 5);
			}

			if (animEventName == "EnemyAttackVFX2")
			{
				Quaternion rotation = Quaternion.LookRotation(attacker.GetForwardVector(), attacker.transform.up - Vector3.Cross(attacker.GetForwardVector(), attacker.transform.up));
				Destroy(Instantiate(attackVFX, attacker.transform.position + attacker.GetForwardVector(), rotation, attacker.transform), 5);
			}
        }

		public override void Ended( Attacker attacker, AttackType attackGroup )
		{
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
