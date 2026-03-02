using DigDig2.Effects;
using UnityEngine;
using UnityEngine.Events;

namespace DigDig2
{
    [RequireComponent(typeof(Attackable))]
    public class Health : MonoBehaviour
    {
        public int MaxHealthPoints
        {
            get
            {
                return maxHealthPoints;
            }
        }
        [Tooltip("Starting health and eventual cap for healing.")]
        [SerializeField] private int maxHealthPoints = 1;

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
        [Tooltip("The entity's current health.")]
        [SerializeField] private int healthPoints = 1;

        [Tooltip("Effects to be played when health is below 0.")]
        [SerializeField] private EffectPlayer deathEffectPlayer;

        [Tooltip("Event is called when health is below 0.")]
        [SerializeField] public UnityEvent death;

        [SerializeField] public UnityEvent<int> healthChanged;



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
            healthChanged.Invoke(healthPoints);
            CheckState();
        }

        public void Kill()
        {
            healthPoints = 0;

            death.Invoke();
            deathEffectPlayer.Play(transform.position, Quaternion.identity, Vector3.one, transform.parent);
            Destroy(gameObject);
        }

        private void CheckState()
        {
            if (healthPoints <= 0) Kill();
        }

        #region Effects

        #endregion
    }
}
