using UnityEngine;

namespace DigDig2.Combat
{
    [CreateAssetMenu( fileName = "AOEAttack", menuName = "Scriptable Objects/Attacks/AOE Attack" )]
    public class AOEAttack : Attack
    {
        [SerializeField] private string animationStateName;
		[SerializeField] private int damage = 1;
        [SerializeField] private float AOEForwardOffset;
        [SerializeField] private GameObject hitboxPrefab;
		[SerializeField] private int bindableAttackHitboxIndex;
		[SerializeField] private GameObject hitEffect;
		[SerializeField] private float knockbackStrength = 50;

		public override void ChargeStart( Attacker attacker, AttackType attackType ) { }

		public override void Charge( Attacker attacker, AttackType attackType, float chargeTime ) { }

		public override void ChargeFull( Attacker attacker, AttackType attackType ) { }

		public override void Trigger( Attacker attacker, AttackType attackGroup, float chargeTime )
		{
			attacker.PlayAnimation( animationStateName );
            Vector3 forwardVector = attacker.GetComponent<EntityCharacterController>().GetForwardVector();
            Vector3 centerOffset = forwardVector * AOEForwardOffset;
            BindableAttackHitbox hitbox = Instantiate(hitboxPrefab, attacker.transform.position + centerOffset, Quaternion.identity).GetComponent<BindableAttackHitbox>();
			attacker.AddMoveSpeedDebuff( animationStateName, attacker.GetBaseMoveSpeed( ) );
			attacker.PushInDirection( Vector3.forward, 10 );
		}

		public override void Ended( Attacker attacker, AttackType attackGroup )
		{
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
