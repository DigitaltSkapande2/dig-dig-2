using UnityEngine;

namespace DigDig2.Combat {
	public class Projectile : MonoBehaviour {
		[SerializeField] private GameObject hitEffect;
		[SerializeField] private LayerMask layerMask;

		private Attacker attacker;
		private string hitboxID;
		private float speed;

		private void Update( ) { transform.position += speed * Time.deltaTime * transform.forward; }

		private void OnTriggerEnter( Collider other ) { DestroyProjectile( ); }

		public void SetInfo( Attack attack, Attacker attacker, float speed, float lifeTime ) {
			hitboxID = Time.time.ToString( );
			this.attacker = attacker;
			this.speed = speed;

			BindableAttackHitbox projectileBindableAttackHitbox = GetComponent<BindableAttackHitbox>( );
			attacker.StartHitboxAttack( attack, hitboxID, projectileBindableAttackHitbox );

			Invoke( nameof( DestroyProjectile ), lifeTime );
		}

		private void DestroyProjectile( ) {
			attacker.EndHitboxAttack( hitboxID );
			Instantiate( hitEffect, transform.position, Quaternion.identity );
			Destroy( gameObject );
		}
	}
}
