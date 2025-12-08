using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace DigDig2
{
    [RequireComponent(typeof(Attackable), typeof(NetworkIdentity))]
    public class Health : NetworkBehaviour
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

        [Tooltip("Event is called when health is below 0.")]
        [SerializeField] private UnityEvent death;



        private void Start()
        {
            if (isServer) SetHealth(healthPoints);
        }

        [Server]
        public void Damage(int damage)
        {
            if (!enabled) return;
            SetHealth(healthPoints - damage);
        }
        [Server]
        public void Heal(int amount)
        {
            if (!enabled) return;
            SetHealth(healthPoints + amount);
        }

        [Server]
        public void SetHealth(int newHealth)
        {
            healthPoints = Mathf.Clamp(newHealth, 0, maxHealthPoints);
            RpcSetHealth(healthPoints);
            CheckState();
        }
        [ClientRpc]
        private void RpcSetHealth(int newHealth)
		{
			healthPoints = newHealth;
		}

        [Server]
        public void Kill()
        {
            healthPoints = 0;

            death.Invoke();
            RpcKill();
            Destroy(gameObject);
        }
        [ClientRpc]
        private void RpcKill()
		{
			death.Invoke();
            PlayDeathEffects();
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
