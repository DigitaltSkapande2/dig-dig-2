using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2
{
    public class Interactor : MonoBehaviour
    {
        [Tooltip("Where the interaction starts from, leave blank to use this GameObject's transform.")]
        [SerializeField] private Transform interactionSource;

        [Tooltip("The angle you have to be in compared to the interactable to interact with it.")]
        [SerializeField, Range(10f, 180f)] private float interactionAngle = 60f;

        [Tooltip("The max distance the source can interact with.")]
        [SerializeField] private float interactionRange = 3f;

        [SerializeField] private GameObject interactionPromptBillboardPrefab;

        private Interactable focusedInteractable;
        private GameObject focusedInteractableInteractionPromptBillboard;
        private List<Interactable> interactablesInRange = new();

        public enum InteractionPhase
        {
            Unknown,
            Began,
            Held,
            Ended,
        }



        private void Update()
        {
            UpdateInteractableFocus();
        }

        private void OnTriggerEnter(Collider other)
        {
            Interactable interactable = other.GetComponent<Interactable>();
            if (interactable)
            {
                if (!interactablesInRange.Contains(interactable)) interactablesInRange.Add(interactable);
                UpdateInteractableFocus();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Interactable interactable = other.GetComponent<Interactable>();
            if (interactable)
            {
                if (interactablesInRange.Contains(interactable)) interactablesInRange.Remove(interactable);
                UpdateInteractableFocus();
            }
        }

        public void UpdateInteractableFocus()
        {
            // Get the interactable with the angle closest to where the player is looking
            Interactable closestInteractable = null;
            float closestInteractableAngle = 180f;
            foreach (Interactable interactableInRange in interactablesInRange)
            {
                float interactableAngle = Vector3.Angle(-interactableInRange.GetInteractableSource().forward, interactionSource.transform.forward);

                if (interactableAngle > interactionAngle) { continue; }

                if (closestInteractable == null)
                {
                    closestInteractable = interactableInRange;
                    closestInteractableAngle = interactableAngle;
                    continue;
                }

                if (interactableAngle < closestInteractableAngle)
                {
                    closestInteractable = interactableInRange;
                    closestInteractableAngle = interactableAngle;
                }
            }

            if (closestInteractable != focusedInteractable)
            {
                if (focusedInteractable) focusedInteractable.SetFocus(false);
                if (focusedInteractableInteractionPromptBillboard) Destroy(focusedInteractableInteractionPromptBillboard.gameObject);

                focusedInteractable = closestInteractable;

                if (focusedInteractable)
                {
                    focusedInteractable.SetFocus(true);
                    focusedInteractableInteractionPromptBillboard = Instantiate(interactionPromptBillboardPrefab, focusedInteractable.transform.position + new Vector3(0f, 2f, 0f), Quaternion.identity);
                }
            }
        }

        public Interactable GetFocusedInteractable()
        {
            return focusedInteractable;
        }

        public bool HasFocusedInteractable()
        {
            return focusedInteractable != null;
        }

        public void SendInteraction(InputActionPhase phase = InputActionPhase.Waiting)
        {
            if (HasFocusedInteractable())
            {
                InteractionPhase interactionPhase = InteractionPhase.Unknown;
                switch (phase)
                {
                    case InputActionPhase.Started: interactionPhase = InteractionPhase.Began; break;
                    case InputActionPhase.Performed: interactionPhase = InteractionPhase.Held; break;
                    case InputActionPhase.Canceled: interactionPhase = InteractionPhase.Ended; break;
                }

                focusedInteractable.Interact(interactionPhase);
            }
        }
    }
}
