using UnityEngine;
using UnityEngine.Events;

namespace DigDig2
{
    public class Damageable : MonoBehaviour
    {
        enum DamageType
        {
            Player,
            Enemy,
            Object
        }

        [SerializeField] DamageType damageType;


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

        void Awake()
        {
            hp = maxHealth;
            Damage(0);
        }

        public void OnHit(AttackData data)
        {
            Damage(data.damage);
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

        void Die()
        {
            Destroy(gameObject);
        }
    }
}
