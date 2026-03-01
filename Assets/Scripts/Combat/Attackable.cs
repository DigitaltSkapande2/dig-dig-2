using System.Collections.Generic;
using DigDig2.Effects;
using UnityEngine;
using UnityEngine.Events;

namespace DigDig2
{
    public class Attackable : MonoBehaviour
    {
        [Tooltip("Duration of invincibility after a hit.")]
        [SerializeField] private float invincibilityTime = 0.05f;

        public string Group
		{
			get
			{
                return group;
			}
		}
        [Tooltip("The group that this attackable is in. Used to filter attacks to only damage certain attackables.")]
        [SerializeField] private string group = "none";

        [Tooltip("Effects that are played when the attackable is hit.")]
        [SerializeField] private EffectPlayer hitEffect;

        [Tooltip("An event that gets emitted when this attackable gets hit.")]
        [SerializeField] public UnityEvent hit = new();

        private float invincibilityTimer = 0;

        private Health healthComponent;
        private EntityCharacterController entityCharacterController;

        

        private void Awake()
        {
            TryGetComponent(out healthComponent);
            TryGetComponent(out entityCharacterController);
        }

        private void Update()
        {
            if (invincibilityTimer > 0)
            {
                invincibilityTimer -= Time.deltaTime;
            }
        }

        public void Hit(Attack attack, Attacker attacker = null)
        {
            if (invincibilityTimer > 0) return;
            invincibilityTimer = invincibilityTime;

            if (attack) attack.Hit(attacker, this, healthComponent, entityCharacterController);

            hit.Invoke();
            hitEffect.Play(transform.position);
        }

        public bool IsInvincible()
        {
            return invincibilityTimer > 0;
        }

        public void ApplyKnockback(Vector3 direction, float strength)
        {
            entityCharacterController.ApplyKnockback(direction, strength);
        }
    }
}