using UnityEngine;

namespace DigDig2
{
	[RequireComponent(typeof(EntityCharacterController), typeof(Animator))]
	public class Attacker : MonoBehaviour
	{
		[SerializeField] private EntityInfo entityInfo; // This should be gotten from the entity character controller instead :3
		[SerializeField] private float chainTimeWindowAfterAttackEnd = 0.25f;
		[SerializeField] private float chainTimeWindowBeforeAttackEnd = 0.25f;

		private Animator animator;
		private EntityCharacterController entityCharacterController;

		private AttackGroup currentChargingAttack;
		private AttackGroup currentPerformingAttack;
		private AttackGroup lastPerformedAttack;
		private float chargeStartTime = -1;
		private float performingAttackEndTime = -1;
		private int currentAttackChain;

		public struct AttackInfo
		{
			public int chainIndex;
			public float lastAttackTime;
		}



		private void Awake()
		{
			entityCharacterController = GetComponent<EntityCharacterController>();
			animator = GetComponent<Animator>();
		}

		private void Update()
        {
            if (IsPerformingAttack() && Time.time >= performingAttackEndTime) EndAttack();
        }

		private AttackGroup GetAttackFromAttackIndex(int attackIndex)
		{
			if (!(entityInfo.attacks.Count > attackIndex)) { Debug.LogWarning($"{entityInfo} does not have an attack win index {attackIndex}."); return null; }

			return entityInfo.attacks[attackIndex];
		}

		#region Charging

		public void ChargeAttack(int attackIndex)
		{
			if (IsPerformingAttack() && !HasMetChainRequirement()) { Debug.LogWarning("An attack is currently being performed and is not being charged, can't attack right now."); return; }
			if (IsChargingAttack()) { Debug.LogWarning("An attack charge is already ongoing, please end it before charging again."); return; }

			EndAttack();
			IncrementAttackChain();

			AttackGroup attack = GetAttackFromAttackIndex(attackIndex);
			if (attack == null) return;
			if (attack.chargeDuration <= 0) { Debug.LogWarning("This attack does not have a charge time."); return; }

			currentChargingAttack = attack;
			chargeStartTime = Time.time;
		}
		public void EndAttackCharge()
		{
			if (!IsChargingAttack()) { Debug.LogWarning("Could not cancel attack charge, there is no attack being charged."); return; }

			currentChargingAttack = null;
			chargeStartTime = -1;
		}
		public bool IsChargingAttack()
		{
			return chargeStartTime != -1;
		}
		public float GetAttackChargeTime()
		{
			if (!IsChargingAttack()) { Debug.LogWarning("There is no attack being charged."); return -1; }
			return Time.time - chargeStartTime;
		}
		public bool IsAttackChargable(int attackIndex)
		{
			AttackGroup attack = GetAttackFromAttackIndex(attackIndex);
			if (attack == null) return false;
			return attack.chargeDuration > 0;
		}
		public bool IsAttackChargable(AttackGroup attack)
		{
			return attack.chargeDuration > 0;
		}
		public bool HasMetAttackChargeRequirement(AttackGroup attack)
		{
			if (!IsChargingAttack()) { Debug.LogWarning("There is no attack being charged."); return false; }
			if (!IsAttackChargable(attack)) return false;
			float chargeTime = GetAttackChargeTime();
			if (attack.requireCharge && chargeTime < attack.chargeDuration) return false;

			return true;
		}

		#endregion

		#region Attacking

		public void Attack(int attackIndex)
		{
			if (IsPerformingAttack() && !HasMetChainRequirement()) { Debug.Log("An attack is currently being performed and is not being charged, can't attack right now."); return; }

			EndAttack();

			if (IsChargingAttack() && HasMetAttackChargeRequirement(currentChargingAttack))
			{
				currentPerformingAttack = currentChargingAttack;
				EndAttackCharge();
			}
			else if (GetAttackFromAttackIndex(attackIndex))
			{
				AttackGroup attack = GetAttackFromAttackIndex(attackIndex);
				if (attack != null && IsAttackChargable(attack) == false) currentPerformingAttack = attack;

				IncrementAttackChain();
			}

			if (currentPerformingAttack == null) { EndAttack(); return; }

			currentPerformingAttack.chain[currentAttackChain].Trigger(this, currentPerformingAttack, Time.time - chargeStartTime);
			performingAttackEndTime = Time.time + currentPerformingAttack.chain[currentAttackChain].GetAttackDuration();
			lastPerformedAttack = currentPerformingAttack;
		}
		public void EndAttack()
		{
			if (currentPerformingAttack == null) { Debug.LogWarning("Can't end attack, no attack is being performed right now."); return; }
			currentPerformingAttack = null;

			Debug.Log("Ended attack.");

			if (IsChargingAttack()) EndAttackCharge();
		}
		public bool IsPerformingAttack()
		{
			return currentPerformingAttack != null;
		}

		#endregion

		#region Chaining

		private void IncrementAttackChain()
		{
			AttackGroup incrementingAttack = currentPerformingAttack;
			if (currentPerformingAttack == null) incrementingAttack = currentChargingAttack;

			if (HasMetChainRequirement() && incrementingAttack.chain.Count - 1 > currentAttackChain)
			{
				currentAttackChain++;
			}
			else
			{
				currentAttackChain = 0;
			}
		}
		public bool HasMetChainRequirement()
		{
			float chainWindowMarginOfError = Time.time - performingAttackEndTime;
			if (!(chainWindowMarginOfError <= chainTimeWindowAfterAttackEnd && chainWindowMarginOfError >= -chainTimeWindowBeforeAttackEnd)) return false;
			if (lastPerformedAttack != currentPerformingAttack) return false;

			return true;
		}

		#endregion

		public EntityCharacterController GetEntityCharacterController()
		{
			return entityCharacterController;
		}
		public Animator GetAnimator()
		{
			return animator;
		}
	}
}
