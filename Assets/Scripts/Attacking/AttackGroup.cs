using System.Collections.Generic;
using UnityEngine;

namespace DigDig2
{
    [CreateAssetMenu(fileName = "AttackGroup", menuName = "Scriptable Objects/Attack Group")]
    public class AttackGroup : ScriptableObject
    {
        public float chargeDuration = 0f;
        public bool requireCharge = false;
        public List<Attack> chain;
    }
}
