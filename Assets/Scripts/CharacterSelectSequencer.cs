using System.Linq;
using UnityEngine;


namespace DigDig2
{
    public class CharacterSelectSequencer : MonoBehaviour
    {
        [Header("Dummies")]
        [SerializeField] private ClickableDummy maxClickableDummy;
        [SerializeField] private SkinnedMeshRenderer[] maxDummyVisuals;
        [SerializeField] private ClickableDummy minisClickableDummy;
        [SerializeField] private SkinnedMeshRenderer[] minisDummyVisuals;
        [Header("Materials")]
        [SerializeField] Material hoverMaterial;


        void Start()
        {
            maxClickableDummy.mouseEnter.AddListener(() =>
            {
                foreach (SkinnedMeshRenderer renderer in maxDummyVisuals)
                {
                    AddRendererMaterial(renderer, hoverMaterial);
                }
            });

            maxClickableDummy.mouseExit.AddListener(() =>
            {
                foreach (SkinnedMeshRenderer renderer in maxDummyVisuals)
                {
                    ResetMaterials(renderer);
                }
            });


        }

        void AddRendererMaterial(SkinnedMeshRenderer renderer, Material hoverMaterial)
        {
            renderer.materials.Append(hoverMaterial);
        }

        void ResetMaterials(SkinnedMeshRenderer renderer)
        {
            renderer.materials[1] = null;
        }
    }
}