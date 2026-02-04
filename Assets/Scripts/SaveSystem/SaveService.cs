using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DigDig2
{
    public class SaveService : Singleton<SaveService> 
    {
        private const string SAVE_DIR = "saves";
        private Dictionary<string, ISaveable> registeredSavables = new();
        private SaveFile loadedSave;
        private bool hasLoadedSave; // FLAG
        private List<string> uniqueNames = new();

#if DEBUG
        [Tooltip("Binds 1-6 keys, to loading saves AND shift+1-6 keys to Saving")]
        [SerializeField] private bool debugKeybinds = false;
#endif

        private struct SaveFile
        {
            public string saveName;
            public string version;
            public Dictionary<string, object> state_data;
        }

        #region UnityCallbacks

#if DEBUG
        void Update()
        {
            if (!debugKeybinds) return;
            bool shift = Input.GetKeyDown(KeyCode.LeftShift);
            string saveName = "";
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                saveName = "1";
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                saveName = "2";
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                saveName = "3";
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                saveName = "4";
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                saveName = "5";
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                saveName = "6";
            }
            
            if (shift) SaveAllAndWriteToFile(saveName);
            else LoadSave(saveName);
        }
#endif


        #endregion
        #region Saving

        public void SaveAllAndWriteToFile(string saveName = "")
        {
            SaveAll();
            WriteSaveToFile(saveName);
        }

        public void WriteSaveToFile(string saveName = "")
        {
            FileSystem.WriteDataToFile(loadedSave, saveName != string.Empty ? saveName : loadedSave.saveName);
        }

        public void SaveAll()
        {
            foreach (KeyValuePair<string, ISaveable> saveablePair in registeredSavables)
            {
                WriteToSaveData(saveablePair.Key, saveablePair.Value.CollectData());
            }

        }

        public void WriteToSaveData(string uniqueName, object data)
        {
            loadedSave.state_data[uniqueName] = data;
        }

        #endregion

        #region LoadSave
        public void LoadSave(string saveName)
        {
            loadedSave = (SaveFile)FileSystem.ReadDataFromFile(Path.Join(SAVE_DIR, saveName));
            if (loadedSave.saveName != saveName)
            {
                Debug.LogWarning("Save Loaded has missmatching saveName property, Overwriting the loaded save's saveName");
                loadedSave.saveName = saveName;
            }
            hasLoadedSave = true;
        }

        public void UnloadCurrentlyLoadedSave()
        {
            hasLoadedSave = false;
        }
        
        #endregion

        #region ISaveable interaction

        public void RegisterSavable(string uniqueName, ISaveable saveable, bool restoreOnRegister = true)
        {
            if (uniqueNames.Contains(uniqueName))
            {
                Debug.LogWarning($"trying to register Savable with already registered uniqueName: {uniqueName}, Aborting");
                return;
            }

            uniqueNames.Add(uniqueName);
            registeredSavables.Add(uniqueName, saveable);

            if (restoreOnRegister) RestoreISavable(uniqueName, saveable);

        }

        public void RestoreISavable(string uniqueName, ISaveable saveable = null)
        {
            if (hasLoadedSave)
            {
                Debug.LogError("Trying to Restore Savable State with no Save Loaded, Be sure to load A save first!");
                return;
            }
            if (saveable == null && registeredSavables.Keys.Contains(uniqueName))
            {
                saveable = registeredSavables[uniqueName];
            }

            if (loadedSave.state_data.Keys.Contains(uniqueName))
            {
                saveable.RestoreState(loadedSave.state_data[uniqueName]);
            }
        }
        
        #endregion
    }
}


