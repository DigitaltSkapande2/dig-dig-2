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
            Attackable damageable = other.GetComponent<Attackable>();
            if (damageable == null) return;

            damageable.Hit(attackData);
        }
    }
}
