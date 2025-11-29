using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public class SceneInitializer : MonoBehaviour
{
    [SerializeField] private bool initOnAwake = true;
    [SerializeField] List<GameObject> prefabsToLoad = new List<GameObject>();
    [SerializeField] List<string> scenesToLoadAdditively = new List<string>();

    [NonSerialized] public Dictionary<string, GameObject> loadedPrefabs = new Dictionary<string, GameObject>();
    [SerializeField] private UnityEvent onSceneInitialized;

    bool hasInitialized = false;

    private void Awake()
    {
        if (!initOnAwake) return;

        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        if (rootObjects.Length != 1)
        {
            Debug.LogWarning("More than one root object in the scene. Please ensure there is Intentional.");
        }

        InitializeScene();
    }

    public async void InitializeScene()
    {
        if (hasInitialized)
        {
            Debug.LogWarning("Scene is already initialized. Re-initialization is not allowed.");
            return;
        }

        foreach (GameObject prefab in prefabsToLoad)
        {
            await loadPrefab(prefab);
        }

        foreach (string scene in scenesToLoadAdditively)
        {
            await SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive).ToUniTask();
            Debug.Log($"Scene '{scene}' loaded additively.");
        }

        onSceneInitialized?.Invoke();
        hasInitialized = true;
    }


    private async UniTask loadPrefab(GameObject prefab)
    {
        await UniTask.Yield();
        if (prefab == null)
        {
            Debug.LogWarning("Prefab is null, cannot instantiate.");
            return;
        }
        loadedPrefabs.Add(prefab.name, Instantiate(prefab));
    }
    

}


// #if UNITY_EDITOR
// [CustomEditor(typeof(SceneInitializer))]
// public class SceneInitializerEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         base.OnInspectorGUI();
//     }
// }

// #endif
