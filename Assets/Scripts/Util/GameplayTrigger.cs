using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace DigDig2
{
    [RequireComponent(typeof(Collider))]
    public class GameplayTrigger : MonoBehaviour
    {
        [SerializeField] UnityEvent triggerEvent;

#if UNITY_EDITOR

        Collider mesh;

        void Start()
        {
            mesh = GetComponent<Collider>();
        }


        void OnDrawGizmos()
        {
            
        }

#endif

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                triggerEvent.Invoke();
            }
        }

        
    }
}
