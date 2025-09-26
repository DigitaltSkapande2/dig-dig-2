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
            Attackable attackable = other.GetComponent<Attackable>();
            if (attackable == null) return;

            attackable.Hit(attackData);
        }
    }
}
