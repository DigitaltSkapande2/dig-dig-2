using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DigDig2
{
    public class SaveManager : Singleton<SaveManager> 
    {
        private const string SAVE_DIR = "saves";
        private Dictionary<string, ISaveable> registeredSavables = new();
        private SaveFile loadedSave = null;
        private bool HasLoadedSave
        {
            get
            {
                return loadedSave != null;
            }
        }
        private List<string> uniqueNames = new();

        private class SaveFile
        {
            public string saveName;
            public string version;
            public Dictionary<string, object> stateData;
        }



        private void Start()
        {
            Debug.Log($"Saving files in: {Path.Join(Application.persistentDataPath, SAVE_DIR)}");

            string saveDirectoryPath = FileSystem.GetFilePath(SAVE_DIR);
            if (!Directory.Exists(saveDirectoryPath)) Directory.CreateDirectory(saveDirectoryPath);

            loadedSave = new SaveFile();
        }

        #region Save Management

        public List<string> GetSaveFileNames()
        {
            List<string> saveFileNames = new();
            List<string> saveFiles = FileSystem.GetFilesInDirectory(SAVE_DIR);
            foreach (string saveFile in saveFiles)
            {
                SaveFile saveFileData = FileSystem.ReadDataFromFile<SaveFile>(Path.Join(SAVE_DIR, Path.GetFileNameWithoutExtension(saveFile)));
                saveFileNames.Add(saveFileData.saveName);
            }

            return saveFileNames;
        }

        #endregion

        #region Saving

        public void SaveAllAndWriteToFile(string saveName = "")
        {
            SaveAll();
            WriteSaveToFile(saveName);
        }

        public void WriteSaveToFile(string saveName = "")
        {
            string currentSaveName = loadedSave.saveName;
            if (saveName != string.Empty) currentSaveName = saveName;

            loadedSave.saveName = currentSaveName;
            loadedSave.version = Application.version;
            FileSystem.WriteDataToFile(Path.Join(SAVE_DIR, currentSaveName), loadedSave);
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
            loadedSave.stateData[uniqueName] = data;
        }

        #endregion

        #region Loading
        public bool LoadSave(string saveName)
        {
            object returnedData = FileSystem.ReadDataFromFile<SaveFile>(Path.Join(SAVE_DIR, saveName));
            if (returnedData != null)
            {
                loadedSave = (SaveFile)returnedData;
                if (loadedSave.saveName != saveName)
                {
                    Debug.LogWarning("Save Loaded has missmatching saveName property, Overwriting the loaded save's saveName");
                    loadedSave.saveName = saveName;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public void UnloadCurrentlyLoadedSave()
        {
            loadedSave = null;
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
            if (HasLoadedSave)
            {
                Debug.LogError("Trying to Restore Savable State with no Save Loaded, Be sure to load A save first!");
                return;
            }
            if (saveable == null && registeredSavables.Keys.Contains(uniqueName))
            {
                saveable = registeredSavables[uniqueName];
            }

            if (loadedSave.stateData.Keys.Contains(uniqueName))
            {
                saveable.RestoreState(loadedSave.stateData[uniqueName]);
            }
        }
        
        #endregion
    }
}


