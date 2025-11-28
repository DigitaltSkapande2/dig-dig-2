using System;
using UnityEngine;
using UnityEngine.Events;

namespace DigDig2
{
    [RequireComponent(typeof(Collider))]
    public class GameplayTrigger : MonoBehaviour
    {
        [SerializeField] UnityEvent triggerEvent;

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                triggerEvent.Invoke();
            }
        }
    }
}
