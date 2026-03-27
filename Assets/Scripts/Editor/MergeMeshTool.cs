using System.Collections.Generic;
using DigDig2.Debugging;
using UnityEditor;
using UnityEngine;

namespace DigDig2
{
    public class MergeMeshTool
    {
        [MenuItem("Tools/MergeMeshes")]
        public static void CombineMeshes()
        {
            BetterDebug.Log("Merging Meshes");

            List<MeshFilter> meshFilters = new List<MeshFilter>();

            foreach (Transform go in Selection.GetTransforms(SelectionMode.Deep | SelectionMode.Editable | SelectionMode.ExcludePrefab))
            {
                if (go.TryGetComponent(out MeshFilter meshFilter))
                {
                    meshFilters.Add(meshFilter);
                }
            }

            Debug.Log($"combining {meshFilters.Count} meshFilters");
            CombineInstance[] instances = new CombineInstance[meshFilters.Count];

            for (int i = 0; i < meshFilters.Count; i++)
            {
                var meshFilter = meshFilters[i];

                instances[i] = new CombineInstance
                {
                    mesh = meshFilter.sharedMesh,
                    transform = meshFilter.transform.localToWorldMatrix,
                };

                meshFilter.gameObject.SetActive(false);
                
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(instances);

            MeshFilter finalMeshFilter;
            if (Selection.activeGameObject.TryGetComponent<MeshFilter>(out finalMeshFilter)) ;
            else
            {
                finalMeshFilter = Selection.activeGameObject.AddComponent<MeshFilter>();
            }
            finalMeshFilter.mesh = combinedMesh;
            finalMeshFilter.gameObject.SetActive(true);
        }

    }
}