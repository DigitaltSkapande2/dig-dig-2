using UnityEngine;

namespace DigDig2
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private GameObject hitEffect;
        private string hitboxID;
        private Attacker attacker;
        private float speed;
        private bool hit;


        private void Update()
        {
            transform.position += speed * Time.deltaTime * transform.forward;
        }

        void OnTriggerEnter(Collider other)
        {
            if (hit) return;
            DestroyProjectile();
        }

        public void SetInfo(Attack attack, Attacker attacker, float speed, float lifeTime)
        {
            hitboxID = Time.time.ToString();
            this.attacker = attacker;
            this.speed = speed;

            BindableAttackHitbox projectileBindableAttackHitbox = GetComponent<BindableAttackHitbox>();
			attacker.StartHitboxAttack(attack, hitboxID, projectileBindableAttackHitbox);

            Invoke(nameof(DestroyProjectile), lifeTime);
        }

        private void DestroyProjectile()
        {
            hit = true;
            attacker.EndHitboxAttack(hitboxID);
            Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
