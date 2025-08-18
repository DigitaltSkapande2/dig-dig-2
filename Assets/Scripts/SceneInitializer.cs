// using System;
// using System.Collections.Generic;
// using System.Data;
// using System.Threading.Tasks;
// using Cysharp.Threading.Tasks;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.SceneManagement;


// public class SceneInitializer : MonoBehaviour
// {
//     [SerializeField] private bool initOnAwake = true;
//     [SerializeField] List<GameObject> prefabsToLoad = new List<GameObject>();
//     [SerializeField] List<SceneAsset> scenesToLoadAdditively = new List<SceneAsset>();

//     [NonSerialized] public Dictionary<string, GameObject> loadedPrefabs = new Dictionary<string, GameObject>();
//     [SerializeField] private UnityEvent onSceneInitialized;

//     bool hasInitialized = false;

//     private void Awake()
//     {
//         if (!initOnAwake) return;

//         GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
//         if (rootObjects.Length != 1)
//         {
//             Debug.LogWarning("More than one root object in the scene. Please ensure there is Intentional.");
//         }

//         InitializeScene();
//     }

//     public async void InitializeScene()
//     {
//         if (hasInitialized)
//         {
//             Debug.LogWarning("Scene is already initialized. Re-initialization is not allowed.");
//             return;
//         }

//         foreach (GameObject prefab in prefabsToLoad)
//         {
//             await loadPrefab(prefab);
//         }

//         foreach (SceneAsset sceneAsset in scenesToLoadAdditively)
//         {
//             string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
//             if (string.IsNullOrEmpty(scenePath))
//             {
//                 Debug.LogWarning($"Scene asset '{sceneAsset.name}' does not have a valid path.");
//                 continue;
//             }

//             await SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive).ToUniTask();
//             Debug.Log($"Scene '{sceneAsset.name}' loaded additively.");
//         }

//         onSceneInitialized?.Invoke();
//         hasInitialized = true;
//     }


//     private async UniTask loadPrefab(GameObject prefab)
//     {
//         await UniTask.Yield();
//         if (prefab == null)
//         {
//             Debug.LogWarning("Prefab is null, cannot instantiate.");
//             return;
//         }
//         loadedPrefabs.Add(prefab.name, Instantiate(prefab));
//     }
    

// }


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
