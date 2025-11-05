using UnityEngine;

namespace DigDig2
{
    public abstract class Attack : ScriptableObject
    {
        [SerializeField] protected float attackDuration = 1f;

        public abstract void Charge(Attacker attacker, AttackType attackGroup);
        public abstract void Trigger(Attacker attacker, AttackType attackGroup, float chargeTime);
        public abstract void Ended(Attacker attacker, AttackType attackGroup);
        public abstract void Hit(Attacker attacker, Attackable attackable, Health healthComponent, EntityCharacterController entityCharacterController);

        public float GetAttackDuration()
        {
            return attackDuration;
        }
    }
}
