using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DigDig2
{
    public class SaveManager : Singleton<SaveManager> 
    {
        private const string SAVES_DIRECTORY_NAME = "saves";
        private const string SAVE_FILE_EXTENSION = ".json";
        private const string NEW_SAVE_PREFIX = "Save";

        private Dictionary<string, ISaveable> registeredSavables = new();
        private GameSave loadedGameSave = null;
        private bool HasLoadedSave
        {
            get
            {
                return loadedGameSave != null;
            }
        }
        private List<string> uniqueNames = new();

        public class GameSave
        {
            public string saveName;
            public string version;
            public Dictionary<string, object> stateData;
        }



        private void Start()
        {
            string saveDirectoryPath = GetSavesDirectoryPath();
            Debug.Log($"Saving files in: {saveDirectoryPath}");
            if (!Directory.Exists(saveDirectoryPath)) Directory.CreateDirectory(saveDirectoryPath);
        }

        #region Directory Management

        public string GetSavesDirectoryPath()
        {
            return Path.Join(FileSystem.GetDataPath(), SAVES_DIRECTORY_NAME);
        }
        public string GetSaveFilePathFromName(string saveName)
        {
            return Path.Join(GetSavesDirectoryPath(), saveName, SAVE_FILE_EXTENSION);
        }

        public List<string> GetSaveFiles()
        {
            return FileSystem.GetFilesInDirectory(GetSavesDirectoryPath());
        }
        public List<string> GetSaveFileSaveNames()
        {
            List<string> saveFileNames = new();
            List<string> saveFiles = GetSaveFiles();
            foreach (string saveFilePath in saveFiles)
            {
                GameSave saveFileData = FileSystem.ReadDataFromFile<GameSave>(saveFilePath);
                saveFileNames.Add(saveFileData.saveName);
            }

            return saveFileNames;
        }

        #endregion

        #region Save Creation

        public void CreateNewSave(string saveName = "")
        {
            if (saveName == string.Empty) saveName = GetNextFreeSaveName();

            loadedGameSave = new()
            {
                saveName = saveName,
                version = Application.version
            };
        }
        public string GetNextFreeSaveName()
        {
            return NEW_SAVE_PREFIX + " " + GetSaveFiles().Count + 1;
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
            string currentSaveName = loadedGameSave.saveName;
            if (saveName != string.Empty) currentSaveName = saveName;

            loadedGameSave.saveName = currentSaveName;
            loadedGameSave.version = Application.version;
            FileSystem.WriteDataToFile(GetSaveFilePathFromName(saveName), loadedGameSave);
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
            loadedGameSave.stateData[uniqueName] = data;
        }

        #endregion

        #region Loading
        public GameSave ReadSaveFile(string saveName)
        {
            return FileSystem.ReadDataFromFile<GameSave>(GetSaveFilePathFromName(saveName));
        }

        public bool LoadSave(GameSave gameSave)
        {
            loadedGameSave = gameSave;

            return true;
        }
        public bool LoadSave(string saveName)
        {
            GameSave gameSave = ReadSaveFile(saveName);
            if (gameSave != null)
            {
                return LoadSave(gameSave);
            }
            else
            {
                return false;
            }
        }

        public void UnloadCurrentlyLoadedSave()
        {
            loadedGameSave = null;
        }
        
        #endregion

        #region ISaveable interaction

        public void RegisterSavable(string uniqueName, ISaveable saveable, bool restoreOnRegister = true)
        {
            if (uniqueNames.Contains(uniqueName))
            {
                Debug.LogWarning($"Trying to register Savable with already registered uniqueName: {uniqueName}, aborting");
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
                Debug.LogError("Trying to restore Savable state with no save loaded, be sure to load a save first!");
                return;
            }
            if (saveable == null && registeredSavables.Keys.Contains(uniqueName))
            {
                saveable = registeredSavables[uniqueName];
            }

            if (loadedGameSave.stateData.Keys.Contains(uniqueName))
            {
                saveable.RestoreState(loadedGameSave.stateData[uniqueName]);
            }
        }
        
        #endregion
    }
}


