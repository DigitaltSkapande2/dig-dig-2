using System.Collections.Generic;

using DigDig2.EffectSystem;
using DigDig2.Util;

using UnityEngine;
using UnityEngine.Events;

namespace DigDig2.Combat
{
	public class Projectile : MonoBehaviour
	{
		private const int POSITION_STEPS = 10;
		
		[SerializeField] AOEAttack hitAttack;
        [SerializeField] private float sphericalRadius;
		[SerializeField] private LayerMask layerMask;
        [SerializeField] private EffectPlayer onHitEffect;
		[SerializeField] private UnityEvent hit;
		
        

		private Attacker attacker;
		private string hitboxID;
		private float speed;

		private TrailRenderer[ ] trails;
		private Rotator[ ] rotators;

		bool hasHit;

		private void Awake( )
		{
			trails = GetComponentsInChildren<TrailRenderer>( );
			rotators = GetComponentsInChildren<Rotator>( );
		}

		private void FixedUpdate( ) 
		{
			if (hasHit) return;
			if (Physics.SphereCast(transform.position, sphericalRadius, transform.forward, out RaycastHit raycastHit, speed * Time.deltaTime, layerMask))
			{
				transform.position = raycastHit.point;
                onHitEffect?.Play(raycastHit.point);
				ProjectileHit();
				hasHit = true;
				return;
			}

			Vector3 positionStep = speed * Time.fixedDeltaTime * transform.forward / POSITION_STEPS;
			for ( int step = 0; step < POSITION_STEPS; step++ )
			{
				transform.position += positionStep;

				foreach ( Rotator rotator in rotators )
				{
					if ( !rotator.handledExternally ) continue;
					rotator.Rotate( 1 / (float)POSITION_STEPS, Time.fixedDeltaTime );
				}

				foreach ( TrailRenderer trail in trails )
				{
					trail.AddPosition( trail.transform.position );
				}
			}
		}

        public void SetInfo( Attack attack, Attacker attacker, float speed, float lifeTime )
		{
			hitboxID = Time.time.ToString( );
			this.attacker = attacker;
			this.speed = speed;

			Invoke( nameof( ProjectileHit ), lifeTime );
		}

		private void ProjectileHit( )
		{
			if (hasHit) return;

			hit.Invoke();
            

			hitAttack.TriggerIndependent(attacker, transform.position, hitboxID, transform);
			attacker.activeAttacks[hitboxID].Trigger(hitboxID);

			attacker.EndHitboxAttack( hitboxID );
			speed = 0;
			Invoke(nameof(DestroyProjectile), 1);
		}

		private void DestroyProjectile()
		{
			Destroy( gameObject );
		}
	}
}
