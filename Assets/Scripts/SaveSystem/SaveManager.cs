using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using Mirror;
using UnityEngine.Events;

namespace DigDig2
{
    public class SaveManager : Singleton<SaveManager>
    {
        private static UnityEvent save = new();
        private static UnityEvent load = new();

        private string SAVE_DIR;

        private string[] savePaths;


        private void Start()
        {
            SAVE_DIR = Application.persistentDataPath;
            compileSaveList();
        }
        
        public void LoadSave(string saveName)
        {
            
        }

        private void compileSaveList()
        {
            if (!Directory.Exists(SAVE_DIR))
            {
                Debug.Log($"Save directory ('{SAVE_DIR}') does not exist, Creating directory...");
                Directory.CreateDirectory(SAVE_DIR);
            }

            savePaths = Directory.GetFiles(SAVE_DIR);
        }

        public string[] GetSaves()
        {
            return savePaths;    
        }

        public void SetSavePoint()
        {
            
        }
    }
}


