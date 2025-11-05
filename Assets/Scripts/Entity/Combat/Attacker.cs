using System.Collections.Generic;
using UnityEngine;

namespace DigDig2
{
	[RequireComponent(typeof(EntityCharacterController), typeof(Animator))]
	public class Attacker : MonoBehaviour
	{
		[Tooltip("The attack types that this entity has access to.")]
		[SerializeField] private List<AttackType> attackTypes;

		[Tooltip("The time after the last attack was finished where you can attack again to trigger a chain.")]
		[SerializeField] private float chainTimeWindowAfterAttackEnd = 0.05f;
		[Tooltip("The time before the last attack is going to finish where you can attack again to trigger a chain.")]
		[SerializeField] private float chainTimeWindowBeforeAttackEnd = 0.5f;

		[Tooltip("The \"anchor points\" that an attack can use to create hitboxes.")]
		[SerializeField] private List<Transform> bindableTransforms = new();

		private Animator animator;

		private AttackType currentChargingAttackType;
		private AttackType currentPerformingAttackType;
		private AttackType lastPerformedAttackType;
		private int lastPerformedAttackIndex;
		private float chargeStartTime = -1;
		private float performingAttackEndTime = -1;
		private int currentAttackChain;
		private bool chainAttackQueued = false;
		private float attackCooldownTimer = 0f;

		private Dictionary<string, AttackHitbox> activeAttackHitboxes = new();

		public struct AttackHitbox
        {
			public Vector3 size;
			public Transform boundTransform;
			public Attack boundAttack;
			public List<Attackable> attackedEnemies;
        }



		private void Awake()
		{
			animator = GetComponent<Animator>();
		}

		private void Update()
		{
			if (IsPerformingAttack() && Time.time >= performingAttackEndTime)
			{
				EndAttack();
				if (chainAttackQueued)
				{
					Attack(lastPerformedAttackIndex, true);
					chainAttackQueued = false;
				}
				else
				{
					attackCooldownTimer = lastPerformedAttackType.endCooldown;
				}
			}

			if (attackCooldownTimer > 0) attackCooldownTimer -= Time.deltaTime;
		}

        private void FixedUpdate()
        {
            foreach (KeyValuePair<string, AttackHitbox> attackHitboxPair in activeAttackHitboxes)
			{
				AttackHitbox attackHitbox = attackHitboxPair.Value;
				Collider[] colliders = Physics.OverlapBox(attackHitbox.boundTransform.position, attackHitbox.size / 2, attackHitbox.boundTransform.rotation);;
				foreach (Collider collider in colliders)
				{
					Attackable attackable = collider.GetComponent<Attackable>();
					if (!attackable) continue;
					if (attackable == this) continue;
					if (attackHitbox.attackedEnemies.Contains(attackable)) continue;

					attackHitbox.attackedEnemies.Add(attackable);
					attackable.Hit(attackHitbox.boundAttack, this);
                }
            }
        }

        private void OnDrawGizmos()
		{
			foreach (KeyValuePair<string, AttackHitbox> attackHitboxPair in activeAttackHitboxes)
			{
				AttackHitbox attackHitbox = attackHitboxPair.Value;
				Gizmos.matrix = attackHitbox.boundTransform.localToWorldMatrix;
				Gizmos.color = Color.red;
				Gizmos.DrawWireCube(Vector3.zero, attackHitbox.size);
            }
        }

        private AttackType GetAttackTypeFromIndex(int attackTypeIndex)
		{
			if (!(attackTypes.Count > attackTypeIndex)) { Debug.LogWarning($"{name} does not have an attack type index of {attackTypeIndex}."); return null; }

			return attackTypes[attackTypeIndex];
		}

		#region Charging

		public void ChargeAttack(int attackTypeIndex)
		{
			if (IsPerformingAttack()) { Debug.LogWarning("An attack is currently being performed and is not being charged, can't attack right now."); return; }
			if (IsChargingAttack()) { Debug.LogWarning("An attack charge is already ongoing, please end it before charging again."); return; }

			EndAttack(true);
			IncrementAttackChain();

			AttackType attackType = GetAttackTypeFromIndex(attackTypeIndex);
			if (attackType == null) return;
			if (attackType.chargeDuration <= 0) { Debug.LogWarning("This attack does not have a charge time."); return; }

			currentChargingAttackType = attackType;
			chargeStartTime = Time.time;
		}
		public void EndAttackCharge()
		{
			if (!IsChargingAttack()) { Debug.LogWarning("Could not cancel attack charge, there is no attack being charged."); return; }

			currentChargingAttackType = null;
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
		public bool IsAttackChargable(int attackTypeIndex)
		{
			AttackType attackType = GetAttackTypeFromIndex(attackTypeIndex);
			if (attackType == null) return false;
			return attackType.chargeDuration > 0;
		}
		public bool IsAttackTypeChargable(AttackType attackType)
		{
			return attackType.chargeDuration > 0;
		}
		public bool HasMetAttackChargeRequirement(AttackType attackType)
		{
			if (!IsChargingAttack()) { Debug.LogWarning("There is no attack being charged."); return false; }
			if (!IsAttackTypeChargable(attackType)) return false;
			float chargeTime = GetAttackChargeTime();
			if (attackType.requireCharge && chargeTime < attackType.chargeDuration) return false;

			return true;
		}

		#endregion

		#region Attacking

		public void Attack(int attackTypeIndex, bool skipChainCheck = false)
		{
			if (attackCooldownTimer > 0) { Debug.LogWarning("Attack is on cooldown, can't attack right now"); return; }

			if (IsPerformingAttack())
			{
				Debug.Log("Checking for chain...");
				if (!skipChainCheck && HasMetChainRequirement(currentAttackChain)) { chainAttackQueued = true; Debug.Log("Queuing chain attack!"); }

				Debug.Log("An attack is currently being performed and is not being charged, can't attack right now.");
				return;
			}

			EndAttack(true);

			if (IsChargingAttack() && HasMetAttackChargeRequirement(currentChargingAttackType))
			{
				currentPerformingAttackType = currentChargingAttackType;
				EndAttackCharge();
			}
			else if (GetAttackTypeFromIndex(attackTypeIndex) != null)
			{
				AttackType attack = GetAttackTypeFromIndex(attackTypeIndex);
				if (attack != null && IsAttackTypeChargable(attack) == false) currentPerformingAttackType = attack;

				IncrementAttackChain();
			}
			if (currentPerformingAttackType == null) { EndAttack(); return; }

			lastPerformedAttackIndex = attackTypeIndex;
			currentPerformingAttackType.chain[currentAttackChain].Trigger(this, currentPerformingAttackType, Time.time - chargeStartTime);
			performingAttackEndTime = Time.time + currentPerformingAttackType.chain[currentAttackChain].GetAttackDuration();
			lastPerformedAttackType = currentPerformingAttackType;
		}
		public void EndAttack(bool ignoreWarning = false)
		{
			if (currentPerformingAttackType == null)
			{
				if (!ignoreWarning) Debug.LogWarning("Can't end attack, no attack is being performed right now.");
				return;
			}
			currentPerformingAttackType.chain[currentAttackChain].Ended(this, currentPerformingAttackType);
			currentPerformingAttackType = null;

			Debug.Log("Ended attack.");

			if (IsChargingAttack()) EndAttackCharge();
		}
		public bool IsPerformingAttack()
		{
			return currentPerformingAttackType != null;
		}

		#endregion

		#region Chaining

		private void IncrementAttackChain()
		{
			if (HasMetChainRequirement())
			{
				currentAttackChain++;
			}
			else
			{
				currentAttackChain = 0;
			}
		}
		public bool HasMetChainRequirement(int attackChain = -1)
		{
			if (attackChain == -1) attackChain = currentAttackChain;

			float chainWindowMarginOfError = Time.time - performingAttackEndTime;
			Debug.Log($"the attack chain: {attackChain}, moe: {chainWindowMarginOfError}");
			if (!(chainWindowMarginOfError <= chainTimeWindowAfterAttackEnd && chainWindowMarginOfError >= -chainTimeWindowBeforeAttackEnd)) return false;
			Debug.Log("pass 1");
			if (lastPerformedAttackType != currentPerformingAttackType) return false;
			Debug.Log("pass 2");
			Debug.Log($"attackChain: {attackChain}, count: {currentPerformingAttackType.chain.Count - 1}");
			if (attackChain >= currentPerformingAttackType.chain.Count - 1) return false;
			Debug.Log("pass 3");

			return true;
		}

		#endregion

		#region Animation

		public void PlayAnimation(string animationStateName)
		{
			animator.Play(animationStateName, -1, 0);
		}

		#endregion

		#region Attack Hit Detection

		public AttackHitbox AddAttackHitbox(Attack attack, string id, Vector3 size, Transform boundTransform)
		{
			AttackHitbox newAttackHitbox = new()
			{
				size = size,
				boundTransform = boundTransform,
				boundAttack = attack,
				attackedEnemies = new()
			};

			activeAttackHitboxes[id] = newAttackHitbox;

			return newAttackHitbox;
		}

		public void RemoveAttackHitbox(string id)
		{
			if (activeAttackHitboxes.ContainsKey(id))
			{
				activeAttackHitboxes.Remove(id);
			}
		}
		
		public Transform GetBindableTransform(int index)
        {
			return bindableTransforms[index];
        }

		#endregion
	}
}
