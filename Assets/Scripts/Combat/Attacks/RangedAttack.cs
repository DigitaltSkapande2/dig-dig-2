using Unity.Mathematics;
using UnityEngine;

namespace DigDig2
{
	[CreateAssetMenu(fileName = "RangedAttack", menuName = "Scriptable Objects/Attacks/Ranged Attack")]
    public class RangedAttack : Attack
	{
		[SerializeField] private string animationStateName;
        [SerializeField] private int damage = 1;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed;
        [SerializeField] private float projectileifetime;

		public override void Charge(Attacker attacker, AttackType attackGroup)
		{

		}
		
		public override void Trigger(Attacker attacker, AttackType attackGroup, float chargeTime)
		{
            attacker.PlayAnimation(animationStateName);
            Vector3 forward = attacker.GetComponent<EntityCharacterController>().GetForwardVector();
            Projectile projectile = Instantiate(projectilePrefab, attacker.transform.position + forward, quaternion.LookRotation(forward, Vector3.up)).GetComponent<Projectile>();
            string id = Time.time.ToString();
            projectile.SetInfo(id, attacker, projectileSpeed, projectileifetime);
			attacker.AddAttackHitbox(this, id, Vector3.one, projectile.transform);
		}

        public override void Ended(Attacker attacker, AttackType attackGroup)
        {

        }

        public override void Hit(Attacker attacker, Attackable attackable, Health healthComponent, EntityCharacterController entityCharacterController)
        {
			if (healthComponent) healthComponent.Damage(damage);
        }
	}
}
