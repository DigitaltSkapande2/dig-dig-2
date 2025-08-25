using UnityEngine;

namespace DigDig2
{
    public class Hitbox : MonoBehaviour
    {
        AttackData attackData;

        public void SetAttackData(AttackData data)
        {
            attackData = data;
        }

        void OnTriggerEnter(Collider other)
        {
            Damageable damageable = other.GetComponent<Damageable>();
            if (damageable == null) return;

            damageable.OnHit(attackData);
        }
    }
}
