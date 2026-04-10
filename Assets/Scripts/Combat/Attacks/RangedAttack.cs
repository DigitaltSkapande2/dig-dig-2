using Cysharp.Threading.Tasks;
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
        // [SerializeField] private AudioClip chargeSFX;
 		[SerializeField] private float projectileSpeed;
		[SerializeField] private float projectileLifetime;

		private GameObject chargeVFXInstance;
        private AudioSource chargeAudioSourceInstance;

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
            onChargeEffect?.Play();
            
            // chargeAudioSourceInstance = new GameObject().AddComponent<AudioSource>();
            // chargeAudioSourceInstance.loop = true;
            // chargeAudioSourceInstance.clip = chargeSFX;
            // chargeAudioSourceInstance.Play();
            // chargeAudioSourceInstance.volume = 0f;
            // FadeAudio(chargeAudioSourceInstance, 0, 1, 1f, false).Forget();
		}

        // private async UniTask FadeAudio(AudioSource source, float from, float to, float time, bool destroyOnEnd)
        // {
        //     float startTime = Time.time;
        //     float elapsed = 0f;
        //     while (elapsed < time)
        //     {
        //         await UniTask.Yield(PlayerLoopTiming.Update);
        //         elapsed = Time.time - startTime;
        //
        //         source.volume = Mathf.Lerp(from, to, elapsed / time);
        //     }
        //
        //     if (destroyOnEnd) Destroy(source.gameObject);
        // }

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
