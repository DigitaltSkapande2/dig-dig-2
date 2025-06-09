using UnityEngine;

namespace DigDig2
{
    public class InteractableTest : MonoBehaviour
    {
        public void Interact(Interactor.InteractionPhase phase)
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

            if (phase == Interactor.InteractionPhase.Unknown) meshRenderer.material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            else if (phase == Interactor.InteractionPhase.Began) meshRenderer.material.color = new Color(0f, 1f, 0f);
            else if (phase == Interactor.InteractionPhase.Held) meshRenderer.material.color = new Color(0f, 1f, 1f);
            else if (phase == Interactor.InteractionPhase.Ended) meshRenderer.material.color = new Color(1f, 0f, 0f);
        }
    }
}
