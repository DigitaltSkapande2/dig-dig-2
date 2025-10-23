using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2
{
	[RequireComponent(typeof(EntityCharacterController), typeof(Animator))]
	public class Attacker : MonoBehaviour
	{
		[SerializeField] private EntityInfo entityInfo; // This should be gotten from the entity character controller instead :3
		[SerializeField] private float chainTimeWindowAfterAttackEnd = 0.05f;
		[SerializeField] private float chainTimeWindowBeforeAttackEnd = 0.5f;

		[SerializeField] private List<Transform> bindableTransforms = new();

		private Animator animator;
		private EntityCharacterController entityCharacterController;

		private AttackGroup currentChargingAttack;
		private AttackGroup currentPerformingAttack;
		private AttackGroup lastPerformedAttack;
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
			entityCharacterController = GetComponent<EntityCharacterController>();
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
					attackCooldownTimer = lastPerformedAttack.endCooldown;
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

        private AttackGroup GetAttackFromAttackIndex(int attackIndex)
		{
			if (!(entityInfo.attacks.Count > attackIndex)) { Debug.LogWarning($"{entityInfo} does not have an attack win index {attackIndex}."); return null; }

			return entityInfo.attacks[attackIndex];
		}

		#region Charging

		public void ChargeAttack(int attackIndex)
		{
			if (IsPerformingAttack()) { Debug.LogWarning("An attack is currently being performed and is not being charged, can't attack right now."); return; }
			if (IsChargingAttack()) { Debug.LogWarning("An attack charge is already ongoing, please end it before charging again."); return; }

			EndAttack(true);
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

		public void Attack(int attackIndex, bool skipChainCheck = false)
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

			lastPerformedAttackIndex = attackIndex;
			currentPerformingAttack.chain[currentAttackChain].Trigger(this, currentPerformingAttack, Time.time - chargeStartTime);
			performingAttackEndTime = Time.time + currentPerformingAttack.chain[currentAttackChain].GetAttackDuration();
			lastPerformedAttack = currentPerformingAttack;
		}
		public void EndAttack(bool ignoreWarning = false)
		{
			if (currentPerformingAttack == null)
			{
				if (!ignoreWarning) Debug.LogWarning("Can't end attack, no attack is being performed right now.");
				return;
			}
			currentPerformingAttack.chain[currentAttackChain].Ended(this, currentPerformingAttack);
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
			if (lastPerformedAttack != currentPerformingAttack) return false;
			Debug.Log("pass 2");
			Debug.Log($"attackChain: {attackChain}, count: {currentPerformingAttack.chain.Count - 1}");
			if (attackChain >= currentPerformingAttack.chain.Count - 1) return false;
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
