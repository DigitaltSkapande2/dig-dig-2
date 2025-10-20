using UnityEngine;

namespace DigDig2
{
    public abstract class Attack : ScriptableObject
    {
        [SerializeField] protected float attackDuration = 1f;
        [SerializeField] protected Animation attackAnimation;

        public abstract void Charge(Attacker attacker, AttackGroup attackGroup);
        public abstract void Trigger(Attacker attacker, AttackGroup attackGroup, float chargeTime);
        public abstract void Hit(Attacker attacker, Attackable attackable, Health healthComponent, EntityCharacterController entityCharacterController);

        public float GetAttackDuration()
        {
            return attackDuration;
        }
    }
}
