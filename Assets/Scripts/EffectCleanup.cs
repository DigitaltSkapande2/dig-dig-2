using UnityEngine;

public class EffectCleanup : MonoBehaviour
{
    [SerializeField] bool killAfterDuration;
    [SerializeField] float timeUntilKill;

    void Start()
    {
        if (killAfterDuration)
        {
            Invoke(nameof(Die), timeUntilKill);
        }
        
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
