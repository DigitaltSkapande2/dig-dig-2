using UnityEngine;

namespace DigDig2
{
	[CreateAssetMenu(fileName = "LogAttack", menuName = "Scriptable Objects/Attacks/Test Attack")]
    public class LogAttack : Attack
	{
		public string testMessage;

		public override void Charge(Attacker attacker, AttackGroup attackGroup)
		{
			
		}

        public override void Hit(Attacker attacker, Attackable attackable, Health healthComponent, EntityCharacterController entityCharacterController)
        {
            
        }

        public override void Trigger(Attacker attacker, AttackGroup attackGroup, float chargeTime)
		{
			Debug.Log(testMessage);
		}
	}
}
