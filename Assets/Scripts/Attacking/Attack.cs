using UnityEngine;

namespace DigDig2
{
    public abstract class Attack : ScriptableObject
    {
        public abstract void Trigger(Attacker attacker, AttackGroup attackGroup, float chargeTime);
    }
}
