using UnityEngine;

namespace DigDig2
{
    [CreateAssetMenu(fileName = "AttackData", menuName = "ScriptableObjects/AttackData")]
    public class AttackData : ScriptableObject
    {
        public AnimationCurve X;
        public AnimationCurve Y;
        public AnimationCurve Z;

        public AnimationCurve speed;

        public float step;
        public float attackTime;

        public int damage;

        public GameObject hitbox;
    }
}
