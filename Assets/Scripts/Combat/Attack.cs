using UnityEngine;
using DigDig2.EffectSystem;

namespace DigDig2.Combat
{
	public abstract class Attack : ScriptableObject
	{
		[SerializeField] protected float attackDuration = 1f;
        [SerializeField] public EffectPlayer onPerformEffect;

		public float AttackDuration
		{
			get => attackDuration;
		}

		public abstract void ChargeStart( Attacker attacker, AttackType attackType );
		public abstract void Charge( Attacker attacker, AttackType attackType, float chargeTime );
		public abstract void ChargeFull( Attacker attacker, AttackType attackType );
		public abstract void Trigger( Attacker attacker, AttackType attackType, float chargeTime );
		public abstract void Ended( Attacker attacker, AttackType attackType );
		public abstract void Hit( Attacker attacker, Attackable attackable, Health healthComponent, EntityCharacterController entityCharacterController );
	}
}
