using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DigDig2
{
    public class Attackable : MonoBehaviour
    {
        [SerializeField] public UnityEvent hit = new();

        [Tooltip("Effects that are played when the entity is hit.")]
        [SerializeField] private List<GameObject> hitEffects = new();

        [SerializeField] private float invincibilityTime = 0.05f;

        private float invincibilityTimer = 0;

        private Health healthComponent;
        private EntityCharacterController entityCharacterController;

        private void Awake()
        {
            healthComponent = GetComponent<Health>();
            entityCharacterController = GetComponent<EntityCharacterController>();
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
            PlayHitEffect();
        }

        public bool IsInvincible()
        {
            return invincibilityTimer > 0;
        }

        private void PlayHitEffect()
        {
            foreach (GameObject hitEffect in hitEffects)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
        }
    }
}