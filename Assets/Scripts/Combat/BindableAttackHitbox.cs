using System.Collections.Generic;
using UnityEngine;

namespace DigDig2
{
    public class BindableAttackHitbox : MonoBehaviour
    {
        public enum AttackHitboxShape
        {
            Box,
            Sphere
        }

        public struct AttackInfo
        {
            public Attacker attacker;
            public Attackable attackerAttackable;
            public Attack attack;
            public List<Attackable> attackedEntities;
        }

        [SerializeField] AttackHitboxShape shape = AttackHitboxShape.Box;
        [SerializeField] Vector3 boxSize = Vector3.one;
        [SerializeField] float sphereRadius = 1.0f;

        Dictionary<string, AttackInfo> activeAttacks = new();



        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            switch ( shape )
            {
                case AttackHitboxShape.Box: Gizmos.DrawWireCube(Vector3.zero, boxSize); break;
                case AttackHitboxShape.Sphere: Gizmos.DrawSphere(Vector3.zero, sphereRadius); break;
            }
        }

        public void StartAttack(string attackId, Attacker attacker, Attack attack)
        {
            Attackable attackerAttackable = attacker.GetComponent<Attackable>();
            
            activeAttacks[attackId] = new AttackInfo()
            {
                attacker = attacker,
                attackerAttackable = attackerAttackable,
                attack = attack,
                attackedEntities = new()
            };
        }

        public void Attack(string attackId)
        {
            AttackInfo attackInfo = activeAttacks[attackId];

            Collider[] colliders = shape switch
            {
                AttackHitboxShape.Box => Physics.OverlapBox(transform.position, boxSize / 2, transform.rotation),
                AttackHitboxShape.Sphere => Physics.OverlapSphere(transform.position, sphereRadius),
                _ => new Collider[0],
            };

            foreach (Collider collider in colliders)
            {
                Attackable enemyAttackable = collider.GetComponent<Attackable>();
                if (!enemyAttackable) continue;
                if (enemyAttackable == attackInfo.attackerAttackable) continue;
                if (attackInfo.attackedEntities.Contains(enemyAttackable)) continue;

                attackInfo.attackedEntities.Add(enemyAttackable);
                enemyAttackable.Hit(attackInfo.attack, attackInfo.attacker);
            }
        }

        public void EndAttack(string attackId)
        {
            activeAttacks.Remove(attackId);
        }
    }
}
