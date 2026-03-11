using System.Collections.Generic;

using UnityEngine;

namespace DigDig2.Combat {
	[CreateAssetMenu( fileName = "AttackType", menuName = "Scriptable Objects/Attack Type" )]
	public class AttackType : ScriptableObject {
		public enum ChargeMode { NoCharge, RequireCharge, RequireFullCharge }

		[SerializeField] public float endCooldown = 0.05f;
		[SerializeField] public float chargeDuration;
		[SerializeField] public ChargeMode chargeMode = ChargeMode.NoCharge;
		[SerializeField] public List<Attack> chain;

		public Attack GetAttackFromIndex( int attackIndex ) => chain[ attackIndex ];
	}
}
