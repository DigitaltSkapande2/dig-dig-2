using UnityEngine;

namespace DigDig2
{
    public class Projectile : MonoBehaviour
    {
        private float speed;
        private float lifetime;
        private string hitboxID;
        private Attacker attacker;

        public void SetInfo(string id, Attacker attacker, float speed, float lifeTime)
        {
            hitboxID = id;
            this.attacker = attacker;
            this.speed = speed;

            Invoke(nameof(DestroyProjectile), lifeTime);
        }

        void DestroyProjectile()
        {
            attacker.RemoveAttackHitbox(hitboxID);
            Destroy(gameObject);
        }

        void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
}
