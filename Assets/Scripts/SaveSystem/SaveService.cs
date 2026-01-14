using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using Mirror;
using UnityEngine.Events;
using UnityEditor;
using System.Linq;

namespace DigDig2
{
    public static class SaveService 
    {
        private const string SAVE_DIR = "saves";
        private static Dictionary<string, ISaveable> registeredSavables = new();
        private static SaveFile loadedSave;
        private static bool hasLoadedSave; // FLAG
        private static List<string> uniqueNames = new();

        private struct SaveFile
        {
            public string saveName;
            public string version;
            public Dictionary<string, object> state_data;
        }

        #region Saving

        public static void SaveAllAndWriteToFile()
        {
            SaveAll();
            WriteSaveToFile();
        }

        public static void WriteSaveToFile()
        {
            FileSystem.WriteDataToFile(loadedSave, loadedSave.saveName);
        }

        public static void SaveAll()
        {
            foreach (KeyValuePair<string, ISaveable> saveablePair in registeredSavables)
            {
                WriteToSaveData(saveablePair.Key, saveablePair.Value.CollectData());
            }

        }

        public static void WriteToSaveData(string uniqueName, object data)
        {
            loadedSave.state_data[uniqueName] = data;
        }

        #endregion

        #region LoadSave
        public static void LoadSave(string saveName)
        {
            loadedSave = (SaveFile)FileSystem.ReadDataFromFile(Path.Join(saveName, SAVE_DIR));
            if (loadedSave.saveName != saveName)
            {
                Debug.LogWarning("Save Loaded has missmatching saveName property, Overwriting the loaded save's saveName");
                loadedSave.saveName = saveName;
            }
            hasLoadedSave = true;
        }

        public static void UnloadCurrentlyLoadedSave()
        {
            hasLoadedSave = false;
        }
        
        #endregion

        #region ISaveable interaction

        public static void RegisterSavable(string uniqueName, ISaveable saveable, bool restoreOnRegister = true)
        {
            if (uniqueNames.Contains(uniqueName))
            {
                Debug.LogError($"trying to register Savable with already registered uniqueName: {uniqueName}");
                return;
            }

            uniqueNames.Add(uniqueName);
            registeredSavables.Add(uniqueName, saveable);

            if (restoreOnRegister) RestoreISavable(uniqueName, saveable);

        }

        public static void RestoreISavable(string uniqueName, ISaveable saveable = null)
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


