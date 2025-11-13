using UnityEngine;

namespace DigDig2
{
    public class Projectile : MonoBehaviour
    {
        private string hitboxID;
        private Attacker attacker;
        private float speed;
        private float lifeTime;

        private float lifetimeTimer = 0;



        private void Update()
        {
            transform.position += speed * Time.deltaTime * transform.forward;

            lifetimeTimer += Time.deltaTime;
            if (lifetimeTimer >= lifeTime)
			{
                DestroyProjectile();
			}
        }

        public void SetInfo(string id, Attacker attacker, float speed, float lifeTime)
        {
            hitboxID = id;
            this.attacker = attacker;
            this.speed = speed;
            this.lifeTime = lifeTime;

            Invoke(nameof(DestroyProjectile), lifeTime);
        }

        private void DestroyProjectile()
        {
            attacker.RemoveAttackHitbox(hitboxID);
            Destroy(gameObject);
        }
    }
}
