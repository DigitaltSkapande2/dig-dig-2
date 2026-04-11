using System;
using DigDig2.Debugging;
using DigDig2.EffectSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

namespace DigDig2.Combat
{
	public class Projectile : MonoBehaviour
	{
		[SerializeField] AOEAttack hitAttack;
        [SerializeField] private float sphericalRadius;
		[SerializeField] private LayerMask layerMask;
        [SerializeField] private EffectPlayer onHitEffect;
		[SerializeField] private UnityEvent hit;
        

		private Attacker attacker;
		private string hitboxID;
		private float speed;

		bool hasHit;
        

        private void Update( ) 
		{
			if (hasHit) return;

			RaycastHit hit;
			if (Physics.SphereCast(transform.position, sphericalRadius, transform.forward, out hit, speed * Time.deltaTime, layerMask))
			{
				transform.position = hit.point;
				ProjectileHit();
				hasHit = true;
				return;
			}

			transform.position += speed * Time.deltaTime * transform.forward; 
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
