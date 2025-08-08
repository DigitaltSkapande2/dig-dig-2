using UnityEngine;
using UnityEngine.Events;

namespace DigDig2
{
    public class Damageable : MonoBehaviour
    {
        [Tooltip("Starting healt and eventual cap for healing")]
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

        public void Damage(int damage)
        {
            hp -= damage;

            if (hp <= 0)
            {
                PlayDeathEffects();
                deathEvent.Invoke();
                Invoke(nameof(Die), 0);
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

        }

        void Die()
        {
            Destroy(gameObject);
        }
    }
}
