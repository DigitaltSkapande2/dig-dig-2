using System;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine;

namespace DigDig2.Effects
{
    [Serializable]
    public struct FootstepMaterialOverride
    {
        public Material material;
        public List<GameObject> footstepSFXPrefab;
    }

    public class FootstepSFXPlayer: MonoBehaviour
    {
        [SerializeField] private GameObject[] EffectPrefab;
        [Header("Sense Overrides")]
        [SerializeField] private bool overrideSense = true;
        [SerializeField] private LayerMask overrideSenseLayer;
        [SerializeField] private Vector3 overrideSenseRaycastVector = Vector3.down;
        [SerializeField] FootstepMaterialOverride[] footstepMaterialOverrides;


        private bool isActive = true;

        public void OnFootStepEvent()
        {
            print("Footstep :#");
            if (!isActive) return;
            GameObject effecToPlay = EffectPrefab[UnityEngine.Random.Range(0, EffectPrefab.Length)];

            if (overrideSense)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, overrideSenseRaycastVector, out hit, overrideSenseRaycastVector.magnitude, overrideSenseLayer))
                {
                    if (TryGetMaterialFromRaycastHit(hit, out Material material))
                    {
                        foreach (FootstepMaterialOverride footstepMaterialOverride in footstepMaterialOverrides)
                        {
                            if (footstepMaterialOverride.material == material)
                            {
                                effecToPlay = footstepMaterialOverride.footstepSFXPrefab[UnityEngine.Random.Range(0, footstepMaterialOverride.footstepSFXPrefab.Count)];
                                break;
                            }
                        }
                    }
                }
            }

            Instantiate(effecToPlay, transform.position, Quaternion.identity);
        }



        private static bool TryGetMaterialFromRaycastHit(RaycastHit hit, out Material material)
        {
            MeshFilter meshFilter = hit.collider.GetComponent<MeshFilter>();
                if (meshFilter == null)
                {
                    material = null;
                    return false;
                }

                Mesh mesh = meshFilter.sharedMesh ?? meshFilter.mesh;
                if (mesh == null)
                {
                    material = null;
                    return false;
                }

                Renderer renderer = meshFilter.GetComponent<Renderer>();
                if (renderer == null)
                {
                    material = null;
                    return false;
                }

                int subMeshIndex = GetSubMeshIndex(mesh, hit.triangleIndex);

                Material[] materials = renderer.sharedMaterials ?? renderer.materials;

                int materialIndex;

                // If there are more materials than there are sub-meshes,
                // Unity renders the last sub-mesh with each of the remaining materials,
                // one on top of the next.
                if (materials.Length > mesh.subMeshCount &&
                    subMeshIndex == mesh.subMeshCount - 1)
                {
                    materialIndex = materials.Length - 1;
                }
                else
                {
                    materialIndex = subMeshIndex;
                }

                material= materials.ElementAtOrDefault(materialIndex);
                return true;
        }

        private static int GetSubMeshIndex(Mesh mesh, int triangleIndex)
        {
            for (int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
            {
                int[] subMeshTriangles = mesh.GetTriangles(subMeshIndex);

                for (int i = 0; i < subMeshTriangles.Length; i += 3)
                {
                    if (subMeshTriangles[i] == mesh.triangles[triangleIndex * 3] && 
                    subMeshTriangles[i + 1] == mesh.triangles[triangleIndex * 3 + 1] &&
                    subMeshTriangles[i + 2] == mesh.triangles[triangleIndex * 3 + 2])
                    {
                        return subMeshIndex;
                    }
                }
            }

            return -1;
        }
    }
}


