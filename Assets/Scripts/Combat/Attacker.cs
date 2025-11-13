using System;
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

		[SerializeField] private bool verboseLogging = false;

		public CombatState State
        {
            get
            {
				return state;
            }
        }
		private CombatState state = CombatState.Idle;

		private AttackType currentPerformingAttackType = null;
		private Attack currentPerformingAttack = null;
		private AttackType lastPerformedAttackType = null;
		private Attack lastPerformedAttack = null;

		private float chargeStartTime = -1;
		private float performanceStartTime = -1;

		private int currentAttackChain = 0;
		private int queuedChainAttackTypeIndex = -1;
		private float attackCooldownTimer = 0;

		[NonSerialized] public AttackType heldAttackType = null;

		private Dictionary<string, AttackHitbox> activeAttackHitboxes = new();

		public struct AttackHitbox
		{
			public Vector3 size;
			public Transform boundTransform;
			public Attack boundAttack;
			public List<Attackable> attackedEnemies;
		}

		public enum CombatState
		{
			Idle,
			Performing,
			Charging,
			FullyCharged,
		}

		private Animator animator;
		private Attackable attackable;



		private void Awake()
		{
			animator = GetComponent<Animator>();
			attackable = GetComponent<Attackable>();
		}

		private void Update()
		{
			if (state == CombatState.Charging)
			{
				currentPerformingAttack.Charge(this, currentPerformingAttackType, Mathf.Clamp(Time.time - chargeStartTime, 0, currentPerformingAttackType.chargeDuration));
				if (Time.time - chargeStartTime >= currentPerformingAttackType.chargeDuration)
				{
					state = CombatState.FullyCharged;
					currentPerformingAttack.ChargeFull(this, currentPerformingAttackType);
				}
			}
			else if (state == CombatState.Performing)
            {
                if (Time.time - performanceStartTime >= currentPerformingAttack.AttackDuration)
                {
					EndAttack(true);
                }
            }

			if (queuedChainAttackTypeIndex >= 0) PerformChainAttack(queuedChainAttackTypeIndex);
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
					Attackable enemyAttackable = collider.GetComponent<Attackable>();
					if (!enemyAttackable) continue;
					if (enemyAttackable == attackable) continue;
					if (attackHitbox.attackedEnemies.Contains(enemyAttackable)) continue;

					attackHitbox.attackedEnemies.Add(enemyAttackable);
					enemyAttackable.Hit(attackHitbox.boundAttack, this);
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

		private void LogVerbose(string message)
        {
			if (verboseLogging) Debug.Log(message);
        }
		
		private void LogWarningVerbose(string message)
		{
			if (verboseLogging) Debug.LogWarning(message);
		}

		private AttackType GetAttackTypeFromIndex(int attackTypeIndex)
		{
			if (!(attackTypes.Count > attackTypeIndex)) { Debug.LogError($"{name} does not have an attack type index of {attackTypeIndex}."); return null; }

			return attackTypes[attackTypeIndex];
		}

		#region Input

		public void RequestAttackStart(int attackTypeIndex)
		{
			heldAttackType = GetAttackTypeFromIndex(attackTypeIndex);
		}
		
		public void RequestAttackEnd()
        {
			heldAttackType = null;
        }

		#endregion

		#region Charging

		private void ChargeAttack(AttackType attackType)
		{
			if (attackType.chargeMode == AttackType.ChargeMode.NoCharge) { Debug.LogError($"Attack type \"{attackType.name}\" is not a chargable attack type."); return; }
			if (attackType.chargeDuration <= 0) Debug.LogWarning($"Attack type \"{attackType.name}\" is being charged, but has a charge duration of 0 or below");

			Attack chargingAttack = attackType.GetAttackFromIndex(currentAttackChain);
			chargingAttack.ChargeStart(this, attackType);

			state = CombatState.Charging;
			chargeStartTime = Time.time;
		}
		public void EndAttackCharge()
		{
			if (!IsChargingAttack()) { Debug.LogError("Could not cancel attack charge, there is no attack being charged."); return; }

			chargeStartTime = -1;
		}
		public bool IsChargingAttack()
		{
			return chargeStartTime != -1;
		}
		public float GetAttackChargeTime()
		{
			if (!IsChargingAttack()) { LogWarningVerbose("There is no attack being charged."); return -1; }
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
			if (!IsChargingAttack()) { Debug.LogError("There is no attack being charged."); return false; }
			if (!IsAttackTypeChargable(attackType)) return false;
			float chargeTime = GetAttackChargeTime();
			if (attackType.requireCharge && chargeTime < attackType.chargeDuration) return false;

			return true;
		}

		#endregion

		#region Attacking

		private void Attack(int attackTypeIndex, bool isChain = false)
		{
			if (!isChain && HasMetChainRequirement(currentAttackChain) && !IsChainAttackQueued()) { LogVerbose("Queuing chain attack!"); queuedChainAttackTypeIndex = attackTypeIndex; return; }
			if (!isChain && (IsPerformingAttack() || attackCooldownTimer > 0)) { LogWarningVerbose("An attack is currently being performed and is not being charged or attacking is on cooldown, can't attack right now."); return; }

			EndAttack(false, true);

			if (IsChargingAttack() && HasMetAttackChargeRequirement(currentChargingAttackType))
			{
				currentPerformingAttackType = currentChargingAttackType;
				EndAttackCharge();
			}
			else if (GetAttackTypeFromIndex(attackTypeIndex) != null)
			{
				AttackType attackType = GetAttackTypeFromIndex(attackTypeIndex);
				if (attackType != null && IsAttackTypeChargable(attackType) == false) currentPerformingAttackType = attackType;

				IncrementAttackChain(attackTypeIndex);
			}
			if (currentPerformingAttackType == null) { EndAttack(false, true); return; }

			Attack attack = currentPerformingAttackType.GetAttackFromIndex(currentAttackChain);
			if (attack == null) { EndAttack(false, true); return; }

			attack.Trigger(this, currentPerformingAttackType, Time.time - chargeStartTime);
			performingAttackEndTime = Time.time + currentPerformingAttackType.chain[currentAttackChain].GetAttackDuration();
			lastPerformedAttackType = currentPerformingAttackType;
		}
		public void EndAttack(bool applyCooldown = false, bool ignoreWarning = false)
		{
			if (currentPerformingAttackType == null)
			{
				if (!ignoreWarning) Debug.LogError("Can't end attack, no attack is being performed right now.");
				return;
			}
			if (applyCooldown) attackCooldownTimer = currentPerformingAttackType.endCooldown;

			currentPerformingAttackType.chain[currentAttackChain].Ended(this, currentPerformingAttackType);
			currentPerformingAttackType = null;
		}
		public bool IsPerformingAttack()
		{
			return currentPerformingAttackType != null;
		}

		#endregion

		#region Chaining

		private void IncrementAttackChain(int attackTypeIndex)
		{
			if (HasMetChainRequirement(attackTypeIndex))
			{
				currentAttackChain++;
			}
			else
			{
				currentAttackChain = 0;
			}
		}
		public bool HasMetChainRequirement(int chainingAttackTypeIndex, int attackChain = -1)
		{
			if (attackChain == -1) attackChain = currentAttackChain;

			AttackType chainingAttackType = GetAttackTypeFromIndex(chainingAttackTypeIndex);
			LogVerbose($"Is it the same as the last? {lastPerformedAttackType == chainingAttackType}");

			float chainWindowMarginOfError = Time.time - performingAttackEndTime;
			LogVerbose($"Attack chain margin of error: {chainWindowMarginOfError}");
			if (!(chainWindowMarginOfError <= chainTimeWindowAfterAttackEnd && chainWindowMarginOfError >= -chainTimeWindowBeforeAttackEnd)) return false;
			if (lastPerformedAttackType != chainingAttackType) return false;
			if (attackChain >= chainingAttackType.chain.Count - 1) return false;

			return true;
		}

		private void PerformChainAttack(int attackTypeIndex)
		{
			if (!IsChainAttackQueued()) { Debug.LogError("Attemped to perform a chain attack when no chain attack was queued."); return; }

			LogVerbose("Performing chain attack");

			AttackType chainingAttackType = GetAttackTypeFromIndex(attackTypeIndex);
			if (chainingAttackType.chargeMode)

			Attack(attackTypeIndex, true);
			queuedChainAttackTypeIndex = -1;
		}
		
		private bool IsChainAttackQueued()
        {
			return queuedChainAttackTypeIndex >= 0;
        }

		#endregion

		#region Animation

		public void PlayAnimation(string animationStateName)
		{
			animator.Play(animationStateName, 0, 0);
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
