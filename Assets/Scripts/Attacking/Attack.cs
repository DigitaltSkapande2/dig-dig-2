using UnityEngine;

namespace DigDig2
{
    public abstract class Attack : ScriptableObject
    {
        [SerializeField] private float attackDuration = 1f;

        public abstract void Charge(Attacker attacker, AttackGroup attackGroup);
        public abstract void Trigger(Attacker attacker, AttackGroup attackGroup, float chargeTime);
    }
}
