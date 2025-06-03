using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneInitializer : MonoBehaviour
{
    [SerializeField] List<GameObject> prefabsToLoad = new List<GameObject>();

    [NonSerialized] public Dictionary<string, GameObject> loadedPrefabs = new Dictionary<string, GameObject>();


    private async void Awake()
    {
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        if (rootObjects.Length != 1)
        {
            Debug.LogWarning("More than one root object in the scene. Please ensure there is Intentional.");
        }


        foreach (GameObject prefab in prefabsToLoad)
        {
            await loadPrefab(prefab);
        }
    }


    private async Task loadPrefab(GameObject prefab)
    {
        await Task.Yield();
        if (prefab == null)
        {
            Debug.LogError("Prefab is null, cannot instantiate.");
            return;
        }
        loadedPrefabs.Add(prefab.name, Instantiate(prefab));
    }
    

}


#if UNITY_EDITOR
[CustomEditor(typeof(SceneInitializer))]
public class SceneInitializerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}

#endif
