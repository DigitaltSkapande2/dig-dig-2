using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


namespace DigDig2
{
    public class CharacterSelectSequencer : MonoBehaviour
    {
        [Header("Clickable Materials")]
        [SerializeField] private Collider maxClickableCollider;
        [SerializeField] private Collider minisClickableCollider;
        [Header("Materials")]
        [SerializeField] private float intensityToSet = 5f;
        [SerializeField] private string float_to_modify = "fresnell_intensity";
        [SerializeField] private Material maxHoverMat;
        [SerializeField] private Material minisHoverMat;
        [SerializeField] Material hoverMaterial;


        private void Start()
        {

        }

        private void FixedUpdate()
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit raycastHit))
            {
                if (raycastHit.collider.gameObject == maxClickableCollider)
                {
                    maxHoverMat.SetFloat(float_to_modify, intensityToSet);
                }
                else
                {
                    maxHoverMat.SetFloat(float_to_modify, 0);
                }

                if (raycastHit.collider.gameObject == maxClickableCollider)
                {
                    minisHoverMat.SetFloat(float_to_modify, intensityToSet);
                }
                else
                {
                    minisHoverMat.SetFloat(float_to_modify, 0);
                }
            }
        }
    }
}
