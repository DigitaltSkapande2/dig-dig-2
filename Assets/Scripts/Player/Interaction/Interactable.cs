using UnityEngine;
using UnityEngine.Events;

namespace DigDig2
{
    public class Interactable : MonoBehaviour
    {
        [SerializeField] private Transform interactableSource;

        [Header("Events")]
        [SerializeField] private UnityEvent onInteract = new();
        [SerializeField] private UnityEvent onFocused = new();
        [SerializeField] private UnityEvent onUnfocused = new();

        private bool focused;



        private void Awake()
        {
            Rigidbody triggerRigidbody = gameObject.AddComponent<Rigidbody>();
            triggerRigidbody.isKinematic = true;
            triggerRigidbody.freezeRotation = true;
            triggerRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }

        public void Interact()
        {

        }

        public void SetFocus(bool isFocused)
        {
            focused = isFocused;
        }

        public bool IsFocused()
        {
            return focused;
        }
    }
}
