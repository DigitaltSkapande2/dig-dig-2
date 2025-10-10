using UnityEngine;

namespace DigDig2
{
    public abstract class Attack : ScriptableObject
    {
        public abstract void Trigger(EntityCharacterController entityCharacterController, AttackGroup attackGroup, float chargeTime);
    }
}
