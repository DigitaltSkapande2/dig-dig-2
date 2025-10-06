using UnityEngine;

namespace DigDig2
{
    [CreateAssetMenu(fileName = "AttackData", menuName = "ScriptableObjects/AttackData")]
    public class AttackData : ScriptableObject
    {
        public Vector3 attackOrigin;

        public int damage;
        public float invincibilityTime;
        public float knockbackPower;
        public float invicibilityTime;
        public float cooldown;
        public AnimationClip animation;
    }
}
