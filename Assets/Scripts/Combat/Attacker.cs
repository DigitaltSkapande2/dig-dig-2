using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace DigDig2
{
	[RequireComponent(typeof(EntityCharacterController), typeof(NetworkIdentity))]
	public class Attacker : NetworkBehaviour
	{
		[Tooltip("The attack types that this entity has access to.")]
		[SerializeField] private List<AttackType> attackTypes;

		[Tooltip("The time after the last attack was finished where you can attack again to trigger a chain.")]
		[SerializeField] private float chainTimeWindowAfterAttackEnd = 0.05f;
		[Tooltip("The time before the last attack is going to finish where you can attack again to trigger a chain.")]
		[SerializeField] private float chainTimeWindowBeforeAttackEnd = 0.5f;

		[Tooltip("The \"anchor points\" that an attack can use to create hitboxes.")]
		[SerializeField] private List<BindableAttackHitbox> bindableAttackHitboxes = new();

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
		private float attackCooldownTimer = 0;

		private AttackType heldAttackType = null;
		private bool attackRequestProcessed = true;
		private int attackRequestChain = 0;

		private Dictionary<string, BindableAttackHitbox> activeAttacks = new();

		public enum CombatState
		{
			Idle,
			Performing,
			Charging,
			FullyCharged,
			OnCooldown,
		}

		private Animator animator;
		private EntityCharacterController entityCharacterController;



		private void Awake()
		{
			animator = GetComponentInChildren<Animator>();
			TryGetComponent(out entityCharacterController);
		}

		private void Update()
		{
			if (authority)
				if ((state == CombatState.Idle || attackRequestChain > 0) && !attackRequestProcessed)
				{
					if (heldAttackType != null)
					{
						currentAttackChain = attackRequestChain;

						if (heldAttackType.chargeMode == AttackType.ChargeMode.NoCharge)
						{
							PerformAttack(heldAttackType);
						}
						else
						{
							ChargeAttack(heldAttackType);
						}
					}

					attackRequestProcessed = true;
					attackRequestChain = -1;
				}

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

				if (attackCooldownTimer > 0) attackCooldownTimer -= Time.deltaTime;
				if (state == CombatState.OnCooldown && attackCooldownTimer <= 0) state = CombatState.Idle;
		}

        private void FixedUpdate()
		{
			if (authority)
			{
				foreach (KeyValuePair<string, BindableAttackHitbox> activeAttack in activeAttacks)
				{
					activeAttack.Value.Attack(activeAttack.Key);
				}
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
			if (!authority) return;

			heldAttackType = GetAttackTypeFromIndex(attackTypeIndex);

			LogVerbose($"Attack request started. Held Attack Type: {heldAttackType}");
			bool isValidChain = HasMetChainRequirement(heldAttackType);
			if (isValidChain)
			{
				attackRequestChain = currentAttackChain + 1;
				LogVerbose("Incrementing chain!");
				EndAttack(false, true);
			}
			else
			{
				LogVerbose("Resetting chain!");
				attackRequestChain = 0;
			}

			LogVerbose($"Attack requested! Held Attack Type: {heldAttackType}, Is Chain: {isValidChain}, State: {state}");
			attackRequestProcessed = false;
		}
		
		public void RequestAttackEnd()
		{
			if (!authority) return;

			// Perform the Charged attack (if there is one and it's valid)
			if (IsChargeValid()) PerformAttack(heldAttackType);

			heldAttackType = null;
			attackRequestProcessed = true;
			attackRequestChain = 0;

			LogVerbose("Attack end requested!");
        }

		#endregion

		#region Charging

		private void ChargeAttack(AttackType attackType)
		{
			if (attackType.chargeMode == AttackType.ChargeMode.NoCharge) { Debug.LogError($"Attack type \"{attackType.name}\" is not a chargable attack type."); return; }
			if (attackType.chargeDuration <= 0) Debug.LogWarning($"Attack type \"{attackType.name}\" is being charged, but has a charge duration of 0 or below");

			Attack chargingAttack = attackType.GetAttackFromIndex(currentAttackChain);
			chargingAttack.ChargeStart(this, attackType);

			currentPerformingAttackType = attackType;
			currentPerformingAttack = chargingAttack;

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
		private bool IsChargeValid()
		{
			return (state == CombatState.Charging && currentPerformingAttackType.chargeMode != AttackType.ChargeMode.RequireFullCharge) || state == CombatState.FullyCharged;
		}

		#endregion

		#region Attacking

		private void PerformAttack(AttackType attackType = null)
		{
			if (IsChargingAttack() && attackType == null) // Attack is charged, end charge and trigger attack
			{
				if (IsChargeValid())
				{
					EndAttackCharge();
				}
				else
				{
					EndAttack();
					return;
				}
			}
			else if (attackType != null) // Attack is not charged, trigger attack
			{
				LogVerbose($"Attacking with chain {currentAttackChain}!");
				currentPerformingAttackType = attackType;
				currentPerformingAttack = currentPerformingAttackType.GetAttackFromIndex(currentAttackChain);
			}
			else
			{
				EndAttack(true);
			}

			if (currentPerformingAttack == null)
			{
				EndAttack(false, true);
				Debug.LogWarning($"Failed to get attack chain, currentAttackChain = {currentAttackChain}, currentPerformingAttackType.chain.Count = {currentPerformingAttackType.chain.Count}.");
				return;
			}

			currentPerformingAttack.Trigger(this, currentPerformingAttackType, Time.time - chargeStartTime);
			state = CombatState.Performing;
			performanceStartTime = Time.time;

			lastPerformedAttackType = currentPerformingAttackType;
			lastPerformedAttack = currentPerformingAttack;
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
			state = CombatState.OnCooldown;
			currentPerformingAttackType = null;
			currentPerformingAttack = null;

			LogVerbose("Attack ended!");
		}
		public bool IsPerformingAttack()
		{
			return currentPerformingAttackType != null;
		}
		public float GetAttackPerformanceTime()
		{
			return Time.time - performanceStartTime;
		}

		#endregion

		#region Chaining

		public bool HasMetChainRequirement(AttackType attackType)
		{
			LogVerbose("Checking attack chain requirement:");
			LogVerbose($"> Is it the same as the last? {lastPerformedAttackType == attackType}");
			if (lastPerformedAttackType != attackType) return false;

			float chainWindowMarginOfError = GetAttackPerformanceTime() - lastPerformedAttack.AttackDuration;
			LogVerbose($"> Is it in the margin of error? {chainWindowMarginOfError <= chainTimeWindowAfterAttackEnd && chainWindowMarginOfError >= -chainTimeWindowBeforeAttackEnd}, MoE: {chainWindowMarginOfError}");
			if (!(chainWindowMarginOfError <= chainTimeWindowAfterAttackEnd && chainWindowMarginOfError >= -chainTimeWindowBeforeAttackEnd)) return false;
			LogVerbose($"> Is it at the end of the chain? {currentAttackChain >= attackType.chain.Count - 1} ({currentAttackChain + 1}, {attackType.chain.Count})");
			if (currentAttackChain + 1 >= attackType.chain.Count) return false;

			LogVerbose("Chain is valid!");

			return true;
		}

		#endregion

		#region Animation

		public void PlayAnimation(string animationStateName)
		{
			animator.Play(animationStateName, 0, 0);
			animator.Play(animationStateName, 1, 0);
		}

		#endregion

		#region Attack Hit Detection

		public void StartHitboxAttack(Attack attack, string id, BindableAttackHitbox bindableAttackHitbox)
		{
			if (!authority) return;

			bindableAttackHitbox.StartAttack(id, this, attack);
			activeAttacks[id] = bindableAttackHitbox;
		}

		public void EndHitboxAttack(string id)
		{
			if (!authority) return;

			if (activeAttacks.ContainsKey(id))
			{
				activeAttacks[id].EndAttack(id);
				activeAttacks.Remove(id);
			}
		}
		
		public BindableAttackHitbox GetBindableAttackHitbox(int index)
		{
			if (!authority) return null;

			return bindableAttackHitboxes[index];
        }

		#endregion

		#region Character Controller Interfacing

		public void AddMoveSpeedDebuff(string debuffId, float debuff)
		{
			if (entityCharacterController) entityCharacterController.AddMoveSpeedDebuff(debuffId, debuff);
		}
		public void RemoveMoveSpeedDebuff(string debuffId)
		{
			if (entityCharacterController) entityCharacterController.RemoveMoveSpeedDebuff(debuffId);
		}
		public float GetBaseMoveSpeed()
		{
			if (entityCharacterController) return entityCharacterController.MoveSpeed;
			else return 0;
		}

		#endregion
	}
}
