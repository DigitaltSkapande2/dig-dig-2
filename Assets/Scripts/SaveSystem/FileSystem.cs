using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace DigDig2
{
    public static class FileSystem
    {
        public static string GetDataPath()
        {
            return Application.persistentDataPath;
        }

        public static void WriteDataToFile(string filePath, object data)
        {
            string extension = Path.GetExtension(filePath);
            string dataString = "";
            switch (extension)
            {
                case ".json":                
                    dataString = JsonConvert.SerializeObject(data, Formatting.Indented); 
                    break;
            }

            File.WriteAllText(filePath, dataString);
        }

        public static T ReadDataFromFile<T>(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            string dataString = File.ReadAllText(filePath);

            switch (extension)
            {
                case ".json":
                    Debug.Log(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<T>(dataString)));
                return JsonConvert.DeserializeObject<T>(dataString);
                default: return default;
            }
        }

        
        public static List<string> GetFilesInDirectory(string directoryPath)
        {
            return Directory.GetFiles(directoryPath).ToList();
        }
    }
}

