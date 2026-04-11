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
        [SerializeField] private AudioClip chargeSFX;
        [SerializeField] private GameObject hitboxPrefab;
		[SerializeField] private GameObject hitEffect;
		[SerializeField] private GameObject attackVFX;
		[SerializeField] private float vfxLifeTime;
		[SerializeField] private float knockbackStrength = 50;

		private float lastChargeTime;
		private string independentID;
        
        private AudioSource chargeAudioSourceInstance;

		public override void ChargeStart( Attacker attacker, AttackType attackType )
		{
			attacker.PlayAnimation( chargeAnimationStateName );
			attacker.AddMoveSpeedDebuff( chargeAnimationStateName, attacker.GetBaseMoveSpeed( ) * (1-chargingMoveSpeedFactor) );
            onChargeEffect?.Play();
            
            chargeAudioSourceInstance = new GameObject().AddComponent<AudioSource>();
            chargeAudioSourceInstance.loop = false;
            chargeAudioSourceInstance.clip = chargeSFX;
            chargeAudioSourceInstance.Play();
		}

		public override void Charge( Attacker attacker, AttackType attackType, float chargeTime )
		{

		}

		public override void ChargeFull( Attacker attacker, AttackType attackType )
		{
            
		}

		public override void Trigger( Attacker attacker, AttackType attackGroup, float chargeTime )
        {
            if (chargeAudioSourceInstance) Destroy(chargeAudioSourceInstance);
            onPerformEffect?.Play();
			lastChargeTime = chargeTime;
			attacker.PlayAnimation( triggerAnimationStateName );
			attacker.PushInDirection( Vector3.forward, 5 );

			Quaternion rotation = Quaternion.LookRotation(attacker.GetForwardVector(), attacker.transform.up);
			Destroy(Instantiate(attackVFX, attacker.transform.position, rotation, attacker.transform), vfxLifeTime);
		}

        public override void AnimationEvent(Attacker attacker, AttackType attackGroup, string animEventName)
        {
			if (animEventName != "TriggerAOE") return;
            
            onHitEffect?.Play();
			Vector3 forwardVector = attacker.GetComponent<EntityCharacterController>().GetForwardVector();
            Vector3 centerOffset = forwardVector * aoeForwardOffset;
			Quaternion rotation = Quaternion.LookRotation(forwardVector, attacker.transform.up);
            BindableAttackHitbox hitbox = Instantiate(hitboxPrefab, attacker.transform.position + centerOffset, rotation).GetComponent<BindableAttackHitbox>();
			Destroy(hitbox.gameObject, 2);
			float radius = Mathf.Lerp(minRaduis, maxRadius, lastChargeTime / attackGroup.chargeDuration);
			hitbox.SetSphereRadius(radius);
			attacker.StartHitboxAttack( this, triggerAnimationStateName, hitbox);
			attacker.PushInDirection( Vector3.forward, -10 );
        }

		public void TriggerIndependent(Attacker attacker, Vector3 position, string hitboxID, Transform parent)
		{
            BindableAttackHitbox hitbox = Instantiate(hitboxPrefab, position, Quaternion.identity, parent).GetComponent<BindableAttackHitbox>();
			hitbox.SetSphereRadius(maxRadius);
			independentID = hitboxID;
			attacker.StartHitboxAttack( this, independentID, hitbox);
		}

		public override void Ended( Attacker attacker, AttackType attackGroup )
		{
            if (chargeAudioSourceInstance) Destroy(chargeAudioSourceInstance.gameObject);
			attacker.EndHitboxAttack( triggerAnimationStateName );
			attacker.RemoveMoveSpeedDebuff(chargeAnimationStateName);
		}

		public override void Hit( Attacker attacker, Attackable attackable, Health healthComponent, EntityCharacterController entityCharacterController )
		{
			if ( hitEffect ) Instantiate( hitEffect, attackable.transform.position, Quaternion.identity );
			if ( healthComponent ) healthComponent.Damage( damage );
			attackable.ApplyKnockback( ( attackable.transform.position - attacker.transform.position ).normalized, knockbackStrength );
		}
    }
}
