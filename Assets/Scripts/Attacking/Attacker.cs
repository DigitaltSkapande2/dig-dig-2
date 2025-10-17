using UnityEngine;

namespace DigDig2
{
	public class Attacker : MonoBehaviour
	{
		[SerializeField] private EntityInfo entityInfo; // This should be gotten from the entity character controller instead :3

		private Animator animator;
		private EntityCharacterController entityCharacterController;

		private AttackGroup currentPerformingAttack;
		private AttackGroup lastPerformedAttack;
		private float chargeStartTime;
		private int currentAttackChain;

		private bool aiming;

		public struct AttackInfo
		{
			public int chainIndex;
			public float lastAttackTime;
		}



		private void Awake()
		{
			animator = GetComponent<Animator>();
			entityCharacterController = GetComponent<EntityCharacterController>();
		}

		private AttackGroup GetAttackFromAttackIndex(int attackIndex)
		{
			if (!(entityInfo.attacks.Count > attackIndex)) { Debug.LogWarning($"{entityInfo} does not have an attack win index {attackIndex}."); return null; }

			return entityInfo.attacks[attackIndex];
		}

		public void ChargeAttack(int attackIndex)
		{
			if (IsChargingAttack()) { Debug.LogWarning("An attack charge is already ongoing, please cancel before charging again."); return; }

			AttackGroup attack = GetAttackFromAttackIndex(attackIndex);
			if (attack == null) return;

			currentPerformingAttack = attack;
			chargeStartTime = Time.time;
		}
		public void CancelAttackCharge() { CancelAttack(); }
		public bool IsChargingAttack()
		{
			return chargeStartTime != -1;
		}

		public void Attack(int attackIndex)
		{
			if (IsPerformingAttack() && !IsChargingAttack()) { Debug.Log("An attack is currently being performed and not being charged, can't attack right now."); return; }

			if (!IsPerformingAttack()) currentPerformingAttack = GetAttackFromAttackIndex(attackIndex);

			if (currentPerformingAttack.chargeTime != 0)
			{
				ChargeAttack(attackIndex);
			}
			
			IncrementAttackChain();

			currentPerformingAttack.chain[currentAttackChain].Trigger(this, currentPerformingAttack, Time.time - chargeStartTime);
			lastPerformedAttack = currentPerformingAttack;
		}
		public void CancelAttack()
		{
			if (currentPerformingAttack == null) { Debug.LogWarning("Can't cancel attack, no attack is being performed right now."); return; }

			CancelAttackCharge();
		}
		private void IncrementAttackChain()
		{
			if (lastPerformedAttack == currentPerformingAttack && currentPerformingAttack.chain.Count - 1 > currentAttackChain)
			{
				currentAttackChain++;
			}
			else
			{
				currentAttackChain = 0;
			}
		}
		public bool IsPerformingAttack()
		{
			return currentPerformingAttack != null;
		}
	}
}
