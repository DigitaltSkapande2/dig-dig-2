using UnityEngine;

namespace DigDig2
{
	[CreateAssetMenu(fileName = "MeleeAttack", menuName = "Scriptable Objects/Attacks/Melee Attack")]
    public class MeleeAttack : Attack
	{
		[SerializeField] private string animationStateName;
		[SerializeField] private int damage = 1;

		public override void Charge(Attacker attacker, AttackGroup attackGroup)
		{

		}
		
		public override void Trigger(Attacker attacker, AttackGroup attackGroup, float chargeTime)
		{
			attacker.PlayAnimation(animationStateName);
			attacker.AddAttackHitbox(this, animationStateName, Vector3.one, attacker.GetBindableTransform(0));
		}

        public override void Ended(Attacker attacker, AttackGroup attackGroup)
        {
            attacker.RemoveAttackHitbox(animationStateName);
        }

        public override void Hit(Attacker attacker, Attackable attackable, Health healthComponent, EntityCharacterController entityCharacterController)
        {
			healthComponent.Damage(damage);
        }
	}
}
