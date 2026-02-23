using DigDig2.Effects;
using UnityEngine;
using UnityEngine.Events;

namespace DigDig2
{
    [RequireComponent(typeof(Attackable))]
    public class Health : MonoBehaviour
    {
        [Tooltip("Starting health and eventual cap for healing.")]
        [SerializeField] private int maxHealthPoints = 1;

        [Tooltip("The entity's current health.")]
        public int HealthPoints
		{
            get
            {
                return healthPoints;
            }
            set
			{
                SetHealth(value);
			}
		}
        [SerializeField] private int healthPoints = 1;

        [Tooltip("Effects to be played when health is below 0.")]
        [SerializeField] private GameObject[] deathEffects;
        [SerializeField] private EffectPlayer deathEffectPlayer;
        [SerializeField] private EffectPlayer hitEffectPlayer;

        [Tooltip("Event is called when health is below 0.")]
        [SerializeField] private UnityEvent death;



        private void Start()
        {
            SetHealth(healthPoints);
        }

        public void Damage(int damage)
        {
            if (!enabled) return;
            SetHealth(healthPoints - damage);
        }
        public void Heal(int amount)
        {
            if (!enabled) return;
            SetHealth(healthPoints + amount);
        }

        public void SetHealth(int newHealth)
        {
            healthPoints = Mathf.Clamp(newHealth, 0, maxHealthPoints);
            CheckState();
        }

        public void Kill()
        {
            healthPoints = 0;

            death.Invoke();
            PlayDeathEffects();
            Destroy(gameObject);
        }

        private void CheckState()
        {
            if (healthPoints <= 0) Kill();
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
