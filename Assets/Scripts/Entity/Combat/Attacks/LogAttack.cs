using UnityEngine;

namespace DigDig2
{
	[CreateAssetMenu(fileName = "LogAttack", menuName = "Scriptable Objects/Attacks/Log Attack")]
    public class LogAttack : Attack
	{
		public string testMessage;

		public override void Charge(Attacker attacker, AttackType attackGroup)
		{

		}

		public override void Trigger(Attacker attacker, AttackType attackGroup, float chargeTime)
		{
			Debug.Log(testMessage);
		}

        public override void Ended(Attacker attacker, AttackType attackGroup)
        {
            
        }

        public override void Hit(Attacker attacker, Attackable attackable, Health healthComponent, EntityCharacterController entityCharacterController)
        {
            
        }
	}
}
