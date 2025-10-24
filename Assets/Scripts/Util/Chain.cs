using System.Linq;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DigDig2
{
    public class Chain : MonoBehaviour
    {
        [SerializeField] Transform[] links;
        [SerializeField] float distance;

        void Update()
        {
            distance = (links[0].position - links[links.Length - 1].position).magnitude / (links.Length - 1);

            for (int i = 0; i < links.Length; i++)
            {
                if (i == 0) continue;

                Vector3 direction = links[i].position - links[i - 1].position;
                links[i].transform.position = links[i - 1].position + direction.normalized * distance;
            }
        }
    }
}
