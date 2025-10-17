using UnityEngine;
using UnityEngine.Events;

namespace DigDig2
{
    [RequireComponent(typeof(Attackable))]
    public class Health : MonoBehaviour
    {
        [Tooltip("Starting health and eventual cap for healing")]
        [SerializeField] int maxHealth;


        [Tooltip("Effects to be played when taking damage")]
        [SerializeField] GameObject[] damagedEffects;


        [Tooltip("Event is called when taking damage")]
        [SerializeField] UnityEvent damagedEvent;


        [Tooltip("Effects to be played when health is below 0")]
        [SerializeField] GameObject[] deathEffects;


        [Tooltip("Event is called when health is below 0")]
        [SerializeField] UnityEvent deathEvent;


        int hp;
        bool invincible;

        void Awake()
        {
            hp = maxHealth;
            Damage(0);

            GetComponent<Attackable>().hit.AddListener(OnHit);
        }

        void OnHit(AttackData data)
        {
            if (invincible) return;

            invincible = true;
            Invoke(nameof(DisableInvincibility), data.invicibilityTime);

            Damage(data.damage);

            EntityCharacterController entity = GetComponent<EntityCharacterController>();
            if (entity != null)
            {
                Vector3 knockbackDirection = (transform.position - data.attackOrigin).normalized;
                entity.ApplyKnockback(knockbackDirection * data.knockbackPower);
                
            }
        }

        void Damage(int damage)
        {
            hp -= damage;

            if (hp <= 0)
            {
                deathEvent.Invoke();
                Invoke(nameof(Die), 0);
                if (deathEffects.Length < 1) return;
                PlayDeathEffects();
                return;
            }

            if (damagedEffects.Length > 0)
            {
                PlayDamagedEffects();
            }
        }

        public void Heal()
        {

        }

        void PlayDamagedEffects()
        {
            foreach (GameObject effect in damagedEffects)
            {
                Instantiate(effect, transform.position, Quaternion.identity);
            }
        }

        void PlayDeathEffects()
        {
            foreach (GameObject effect in deathEffects)
            {
                Instantiate(effect, transform.position, Quaternion.identity);
            }
        }

        void DisableInvincibility()
        {
            invincible = false;
        }

        void Die()
        {
            Destroy(gameObject);
        }
    }
}
