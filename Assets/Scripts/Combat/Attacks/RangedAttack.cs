using Unity.Mathematics;
using DigDig2.Entity;
using UnityEngine;

namespace DigDig2.Combat.Attacks
{
	[CreateAssetMenu( fileName = "RangedAttack", menuName = "Scriptable Objects/Attacks/Ranged Attack" )]
	public class RangedAttack : Attack
	{
		[SerializeField] private string chargeAnimationStateName;
		[SerializeField] private string triggerAnimationStateName;
		[SerializeField] private int damage = 1;
		[SerializeField] private float chargingMoveSpeedFactor;
		[SerializeField] private GameObject projectilePrefab;
		[SerializeField] private GameObject chargeVFX;
		[SerializeField] private float projectileSpeed;
		[SerializeField] private float projectileLifetime;

		private GameObject chargeVFXInstance;

		public override void ChargeStart( Attacker attacker, AttackType attackType )
		{
			attacker.PlayAnimation( chargeAnimationStateName );
			attacker.AddMoveSpeedDebuff( chargeAnimationStateName, attacker.GetBaseMoveSpeed( ) * (1-chargingMoveSpeedFactor) );

			Vector3 rayOrigin = attacker.transform.position + attacker.GetForwardVector() * 0.5f;
			RaycastHit hit;
			if (Physics.Raycast(rayOrigin, -attacker.transform.up, out hit, 5, LayerMask.GetMask("Ground")))
			{
				Quaternion rotation = Quaternion.LookRotation(attacker.GetForwardVector(), attacker.transform.up);
				chargeVFXInstance = Instantiate(chargeVFX, hit.point, rotation);
				Destroy(chargeVFXInstance, 5);
			}
		}

		public override void Charge( Attacker attacker, AttackType attackType, float chargeTime ) { }

		public override void ChargeFull( Attacker attacker, AttackType attackType )
		{
			attacker.RemoveMoveSpeedDebuff(chargeAnimationStateName);
			attacker.AddMoveSpeedDebuff(chargeAnimationStateName, attacker.GetBaseMoveSpeed( ) / 2);
		}

		public override void Trigger( Attacker attacker, AttackType attackGroup, float chargeTime )
		{
			attacker.PlayAnimation( triggerAnimationStateName );
			attacker.AddMoveSpeedDebuff( triggerAnimationStateName, attacker.GetBaseMoveSpeed( ) / 2 );
		}

		public override void AnimationEvent(Attacker attacker, AttackType attackGroup, string animEventName)
		{
			if (animEventName == "Trigger")
			{
				Debug.Log( "Hello i am a ranged attack" );
				onPerformEffect?.Play();
				Vector3 forward = attacker.GetComponent<EntityCharacterController>( ).GetForwardVector( );
				Projectile projectile = Instantiate( projectilePrefab, attacker.transform.position, quaternion.LookRotation( forward, Vector3.up ) ).GetComponent<Projectile>( );
				projectile.SetInfo( this, attacker, projectileSpeed, projectileLifetime );
			}
		}

		public override void Ended( Attacker attacker, AttackType attackGroup ) 
		{
			if (chargeVFXInstance && chargeVFXInstance.transform.GetChild(0).TryGetComponent(out Animator animator)) animator.enabled = false; 

			attacker.RemoveMoveSpeedDebuff( chargeAnimationStateName ); 
			attacker.RemoveMoveSpeedDebuff( triggerAnimationStateName );
		}

		public override void Hit( Attacker attacker, Attackable attackable, Health healthComponent, EntityCharacterController entityCharacterController )
		{
			if ( healthComponent ) healthComponent.Damage( damage );
		}
	}
}
