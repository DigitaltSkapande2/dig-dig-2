using UnityEngine;
using UnityEngine.Events;

namespace DigDig2
{
    public class Interactable : MonoBehaviour
    {
        [SerializeField] private Transform interactableSource;

        [Header("Events")]
        [SerializeField] private UnityEvent<Interactor.InteractionPhase> onInteract = new();
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

        public Transform GetInteractableSource()
        {
            return interactableSource != null ? interactableSource : transform;
        }

        public void Interact(Interactor.InteractionPhase phase = Interactor.InteractionPhase.Unknown)
        {
            onInteract.Invoke(phase);
        }

        public void SetFocus(bool isFocused)
        {
            focused = isFocused;

            if (isFocused) { onFocused.Invoke(); } else { onUnfocused.Invoke(); }
        }

        public bool IsFocused()
        {
            return focused;
        }
    }
}
