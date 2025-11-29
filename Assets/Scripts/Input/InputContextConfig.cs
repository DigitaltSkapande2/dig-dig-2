using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif


// SUMMARY
// Lets you assign InputContexts to InputActionMaps in an InputActionAsset

namespace DigDig2
{
    [CreateAssetMenu(fileName = "InputContextConfig", menuName = "Input/Context Config")]
    public class InputContextConfig : ScriptableObject
    {
        public InputActionAsset inputActions;

        [System.Serializable]
        public struct MapContext
        {
            public string mapName;
            public InputContext context;
        }

        public MapContext[] mapContexts;






    }
#if UNITY_EDITOR
    [CustomEditor(typeof(InputContextConfig))]
    public class InputContextConfigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Sync Maps"))
            {
                SyncMaps();
            }
        }
        
        private void SyncMaps()
        {
            InputContextConfig config = (InputContextConfig)target;

            if (config.inputActions == null) return;

            InputContextConfig.MapContext[] previousMapContext = config.mapContexts;

            var maps = config.inputActions.actionMaps;

            config.mapContexts = new InputContextConfig.MapContext[maps.Count];
            for (int i = 0; i < maps.Count; i++)
            {
                if (previousMapContext != null)
                {
                    // Check if map was already Existeding in previous array
                    var existing = System.Array.Find(previousMapContext, m => m.mapName == maps[i].name);
                    if (existing.mapName == maps[i].name)
                    {
                        config.mapContexts[i] = existing;
                        continue;
                    }
                }
                config.mapContexts[i].mapName = maps[i].name;
            }

            EditorUtility.SetDirty(this);
        }
    }

#endif


    [System.Flags]
    public enum InputContext
    {
        None = 0,
        PauseMenu = 1 << 0,
        Gameplay = 1 << 1,
        Cutscene = 1 << 2,
        DebugConsole = 1 << 3,
        DebugNotes = 1 << 4
    }
}