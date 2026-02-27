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
            public bool hasCheckedOnce;
            public Vector3 lastPosition;
            public Quaternion lastRotation;
        }

        [SerializeField] private AttackHitboxShape shape = AttackHitboxShape.Box;
        [SerializeField] private Vector3 boxSize = Vector3.one;
        [SerializeField] private float sphereRadius = 1.0f;
        [SerializeField] private float unitsPerIntermediateCheck = 0.001f;

        private Dictionary<string, AttackInfo> activeAttacks = new();



        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            if (activeAttacks.Count > 0) Gizmos.color = Color.blue;
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
                attackedEntities = new(),
                hasCheckedOnce = false
            };
        }

        public void Attack(string attackId)
        {
            AttackInfo attackInfo = activeAttacks[attackId];

            float distanceBetweenChecks = 1f;
            int intermediateAttacks = 0;
            //Debug.Log("Has attacked once: " + attackInfo.hasCheckedOnce);
            if (attackInfo.hasCheckedOnce)
            {
                distanceBetweenChecks = Vector3.Distance(attackInfo.lastPosition, transform.position);
                intermediateAttacks = Mathf.CeilToInt(distanceBetweenChecks / unitsPerIntermediateCheck);
            }
            
            //Debug.Log("Intermediate attacks: " + intermediateAttacks);
            
            for (int intermediateAttackIndex = 0;
                 intermediateAttackIndex < intermediateAttacks;
                 intermediateAttackIndex++)
            {
                Vector3 intermediatePosition = Vector3.Lerp(attackInfo.lastPosition, transform.position, (float)intermediateAttackIndex / intermediateAttacks);
                Quaternion intermediateRotation = Quaternion.Slerp(attackInfo.lastRotation, transform.rotation, (float)intermediateAttackIndex / intermediateAttacks);
                Debug.DrawLine(intermediatePosition, intermediatePosition + Vector3.up / 10f, Color.blue, 1f);
                Collider[] colliders = shape switch
                {
                    AttackHitboxShape.Box => Physics.OverlapBox(intermediatePosition, boxSize / 2, intermediateRotation),
                    AttackHitboxShape.Sphere => Physics.OverlapSphere(intermediatePosition, sphereRadius),
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
            
            attackInfo.lastPosition = transform.position;
            attackInfo.lastRotation = transform.rotation;
            attackInfo.hasCheckedOnce = true;
            activeAttacks[attackId] = attackInfo;
        }

        public void EndAttack(string attackId)
        {
            activeAttacks.Remove(attackId);
        }
    }
}
