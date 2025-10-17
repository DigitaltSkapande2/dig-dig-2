using UnityEngine;

namespace DigDig2
{
	[CreateAssetMenu(fileName = "TestAttack", menuName = "Scriptable Objects/Attacks/Test Attack")]
    public class TestAttack : Attack
	{
		public string testMessage;

		public override void Charge(Attacker attacker, AttackGroup attackGroup)
		{
			throw new System.NotImplementedException();
		}

		public override void Trigger(Attacker attacker, AttackGroup attackGroup, float chargeTime)
		{
			Debug.Log(testMessage);
		}
	}
}
