using UnityEngine;

namespace DigDig2
{
	[CreateAssetMenu(fileName = "MeleeAttack", menuName = "Scriptable Objects/Attacks/Melee Attack")]
    public class MeleeAttack : Attack
	{
		[SerializeField] private string animationStateName;
		[SerializeField] private int damage = 1;
		[SerializeField] private int bindableAttackHitboxIndex = 0;

		public override void ChargeStart(Attacker attacker, AttackType attackType)
		{
			
		}

		public override void Charge(Attacker attacker, AttackType attackType, float chargeTime)
		{
			
		}

		public override void ChargeFull(Attacker attacker, AttackType attackType)
		{
			
		}
		
		public override void Trigger(Attacker attacker, AttackType attackGroup, float chargeTime)
		{
			attacker.PlayAnimation(animationStateName);
			attacker.StartHitboxAttack(this, animationStateName, attacker.GetBindableAttackHitbox(bindableAttackHitboxIndex));
			attacker.AddMoveSpeedDebuff(animationStateName, attacker.GetBaseMoveSpeed());
		}

        public override void Ended(Attacker attacker, AttackType attackGroup)
        {
            attacker.EndHitboxAttack(animationStateName);
			attacker.RemoveMoveSpeedDebuff(animationStateName);
        }

        public override void Hit(Attacker attacker, Attackable attackable, Health healthComponent, EntityCharacterController entityCharacterController)
		{
			if (healthComponent) healthComponent.Damage(damage);
        }
	}
}
