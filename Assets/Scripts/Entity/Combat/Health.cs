using UnityEngine;
using UnityEngine.Events;

namespace DigDig2
{
    [RequireComponent(typeof(Attackable))]
    public class Health : MonoBehaviour
    {
        [Tooltip("Starting health and eventual cap for healing.")]
        [SerializeField] private int maxHealth = 1;

        [Tooltip("The entity's current health.")]
        [SerializeField] private int health = 1;

        [Tooltip("Effects to be played when health is below 0.")]
        [SerializeField] private GameObject[] deathEffects;

        [Tooltip("Event is called when health is below 0.")]
        [SerializeField] private UnityEvent deathEvent;



        private void Start()
        {
            SetHealth(health);
        }

        public void Damage(int damage)
        {
            SetHealth(health - damage);
        }
        public void Heal(int amount)
        {
            SetHealth(health + amount);
        }

        public void SetHealth(int newHealth)
        {
            health = Mathf.Clamp(newHealth, 0, maxHealth);
            CheckState();
        }

        public void Kill()
        {
            health = 0;

            deathEvent.Invoke();
            PlayDeathEffects();
            Destroy(gameObject);
        }

        private void CheckState()
        {
            if (health <= 0) Kill();
        }

        #region Effects

        private void PlayDeathEffects()
        {
            foreach (GameObject deathEffect in deathEffects)
            {
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            }
        }

        #endregion
    }
}
