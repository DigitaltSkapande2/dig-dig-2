using System.Collections.Generic;
using UnityEngine;

namespace DigDig2
{
    [CreateAssetMenu(fileName = "AttackType", menuName = "Scriptable Objects/Attack Group")]
    public class AttackType : ScriptableObject
    {
        [SerializeField] public float endCooldown = 0.05f;
        [SerializeField] public float chargeDuration = 0f;
        [SerializeField] public bool requireCharge = false;
        [SerializeField] public List<Attack> chain;
    }
}
