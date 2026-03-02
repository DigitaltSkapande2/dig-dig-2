using UnityEngine;
using Random = UnityEngine.Random;

namespace DigDig2
{
    [RequireComponent(typeof(MeshRenderer), typeof(Attackable))]
    public class AttackIndicator : MonoBehaviour
    {
        private MeshRenderer meshRenderer;
        private Attackable attackable;
        
        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            
            attackable = GetComponent<Attackable>();
            attackable.hit.AddListener(OnAttack);
        }

        private void OnAttack()
        {
            meshRenderer.material.color = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }
    }
}
