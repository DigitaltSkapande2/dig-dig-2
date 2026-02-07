using UnityEngine;

namespace DigDig2
{
    public abstract class Attack : ScriptableObject
    {
        public float AttackDuration
        {
            get
            {
                return attackDuration;
            }
        }
        [SerializeField] protected float attackDuration = 1f;

        public abstract void ChargeStart(Attacker attacker, AttackType attackType);
        public abstract void Charge(Attacker attacker, AttackType attackType, float chargeTime);
        public abstract void ChargeFull(Attacker attacker, AttackType attackType);
        public abstract void Trigger(Attacker attacker, AttackType attackType, float chargeTime);
        public abstract void Ended(Attacker attacker, AttackType attackType);
        public abstract void Hit(Attacker attacker, Attackable attackable, Health healthComponent, EntityCharacterController entityCharacterController);
    }
}
