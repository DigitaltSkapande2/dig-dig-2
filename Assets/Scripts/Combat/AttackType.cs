using System.Collections.Generic;
using UnityEngine;

namespace DigDig2
{
    [CreateAssetMenu(fileName = "AttackType", menuName = "Scriptable Objects/Attack Type")]
    public class AttackType : ScriptableObject
    {
        [SerializeField] public float endCooldown = 0.05f;
        [SerializeField] public float chargeDuration = 0f;
        [SerializeField] public ChargeMode chargeMode = ChargeMode.NoCharge;
        [SerializeField] public List<Attack> chain;

        public enum ChargeMode
        {
            NoCharge,
            RequireCharge,
            RequireFullCharge,
        }

        public Attack GetAttackFromIndex(int attackIndex)
        {
            return chain[attackIndex];
        }
    }
}
