using System.Collections.Generic;
using UnityEngine;

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
        [SerializeField] private List<Interactable> interactablesInRange = new();

        private GameObject interactionPromptBillboard;



        private void Awake()
        {
            interactionPromptBillboard = Instantiate(interactionPromptBillboardPrefab);
            interactionPromptBillboard.SetActive(false);
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
            
        }

        public Interactable GetFocusedInteractable()
        {
            return focusedInteractable;
        }

        public bool HasFocusedInteractable()
        {
            return focusedInteractable != null;
        }
    }
}
