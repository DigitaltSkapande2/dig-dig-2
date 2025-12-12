using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using Mirror;
using UnityEngine.Events;

namespace DigDig2
{
    public class SaveManager : MonoBehaviour
    {
        private static UnityEvent save = new();
        private static UnityEvent load = new();

        //private List<Path> savePaths = new();

        private void Start()
        {
            
        }
        
        public void LoadSave(string saveName)
        {
            
        }
    }
}


