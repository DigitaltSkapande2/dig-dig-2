using DigDig2.EffectSystem;
using UnityEngine;

namespace DigDig2.Combat
{
	public class Projectile : MonoBehaviour
	{
		[SerializeField] AOEAttack hitAttack;
		[SerializeField] private GameObject hitEffect;
		[SerializeField] private LayerMask layerMask;
        [SerializeField] private EffectPlayer onHitEffect;

		private Attacker attacker;
		private string hitboxID;
		private float speed;

		private void Update( ) 
		{
			RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.forward, out hit, speed * Time.deltaTime, layerMask))
			{
				hitAttack.TriggerIndependent(attacker, hit.point, hitboxID, transform);
				attacker.activeAttacks[hitboxID].Trigger(hitboxID);
				transform.position = hit.point;
				DestroyProjectile();
			}

			transform.position += speed * Time.deltaTime * transform.forward; 
		}

		public void SetInfo( Attack attack, Attacker attacker, float speed, float lifeTime )
		{
			hitboxID = Time.time.ToString( );
			this.attacker = attacker;
			this.speed = speed;

			Invoke( nameof( DestroyProjectile ), lifeTime );
		}

		private void DestroyProjectile( )
		{
			attacker.EndHitboxAttack( hitboxID );
			Instantiate( hitEffect, transform.position, Quaternion.identity );
			Destroy( gameObject );
		}
	}
}
