using UnityEngine;
using DigDig2.Entity;


namespace DigDig2.Combat
{
    [CreateAssetMenu( fileName = "AOEAttack", menuName = "Scriptable Objects/Attacks/AOE Attack" )]
    public class AOEAttack : Attack
    {
        [SerializeField] private string chargeAnimationStateName;
		[SerializeField] private string triggerAnimationStateName;
		[SerializeField] private float minRaduis;
		[SerializeField] private float maxRadius;
		[SerializeField] private int damage = 1;
        [SerializeField] private float aoeForwardOffset;
		[SerializeField] private float chargingMoveSpeedFactor;
        [SerializeField] private GameObject hitboxPrefab;
		[SerializeField] private GameObject hitEffect;
		[SerializeField] private GameObject attackVFX;
		[SerializeField] private float knockbackStrength = 50;

		private Vector3 lastHitboxPosition;
		private float lastChargeTime;
		private string independentID;

		public override void ChargeStart( Attacker attacker, AttackType attackType )
		{
			attacker.PlayAnimation( chargeAnimationStateName );
			attacker.AddMoveSpeedDebuff( chargeAnimationStateName, attacker.GetBaseMoveSpeed( ) * (1-chargingMoveSpeedFactor) );
		}

		public override void Charge( Attacker attacker, AttackType attackType, float chargeTime )
		{

		}

		public override void ChargeFull( Attacker attacker, AttackType attackType )
		{

		}

		public override void Trigger( Attacker attacker, AttackType attackGroup, float chargeTime )
		{
			lastChargeTime = chargeTime;
			attacker.PlayAnimation( triggerAnimationStateName );
			attacker.PushInDirection( Vector3.forward, 5 );

			Quaternion rotation = Quaternion.LookRotation(attacker.GetForwardVector(), attacker.transform.up);
			Destroy(Instantiate(attackVFX, attacker.transform.position, rotation, attacker.transform), 5);
		}

        public override void AnimationEvent(Attacker attacker, AttackType attackGroup, string animEventName)
        {
			if (animEventName != "TriggerAOE") return;
			
			onPerformEffect?.Play();
			Vector3 forwardVector = attacker.GetComponent<EntityCharacterController>().GetForwardVector();
            Vector3 centerOffset = forwardVector * aoeForwardOffset;
            BindableAttackHitbox hitbox = Instantiate(hitboxPrefab, attacker.transform.position + centerOffset, Quaternion.identity).GetComponent<BindableAttackHitbox>();
			lastHitboxPosition = hitbox.transform.position;
			float radius = Mathf.Lerp(minRaduis, maxRadius, lastChargeTime / attackGroup.chargeDuration);
			hitbox.SetSphereRadius(radius);
			attacker.StartHitboxAttack( this, triggerAnimationStateName, hitbox);
			attacker.PushInDirection( Vector3.forward, -10 );
        }

		public void TriggerIndependent(Attacker attacker, Vector3 position, string hitboxID, Transform parent)
		{
            BindableAttackHitbox hitbox = Instantiate(hitboxPrefab, position, Quaternion.identity, parent).GetComponent<BindableAttackHitbox>();
			independentID = hitboxID;
			attacker.StartHitboxAttack( this, independentID, hitbox);
		}

		public override void Ended( Attacker attacker, AttackType attackGroup )
		{
			attacker.EndHitboxAttack( triggerAnimationStateName );
			attacker.RemoveMoveSpeedDebuff(chargeAnimationStateName);
		}

		public override void Hit( Attacker attacker, Attackable attackable, Health healthComponent, EntityCharacterController entityCharacterController )
		{
			if ( hitEffect ) Instantiate( hitEffect, attackable.transform.position, Quaternion.identity );
			if ( healthComponent ) healthComponent.Damage( damage );
			attackable.ApplyKnockback( ( attackable.transform.position - lastHitboxPosition ).normalized, knockbackStrength );
		}
    }
}
