using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DigDig2
{
    public static class FileSystem
    {


        public static void WriteDataToFile(string fileName, object data)
        {
            string filePath = GetFilePath(fileName + ".json");
            string dataText = JsonUtility.ToJson(data);

            File.WriteAllText(filePath, dataText);
        }

        public static T ReadDataFromFile<T>(string fileName)
        {
            string filePath = GetFilePath(fileName + ".json");
            string dataAsText = File.ReadAllText(filePath);

            return JsonUtility.FromJson<T>(dataAsText); // SERIALIZATION ABSTRACTION???
        }

        public static string GetFilePath(string fileName)
        {
            return Path.Join(Application.persistentDataPath, fileName);
        }
        
        public static List<string> GetFilesInDirectory(string directoryPath)
        {
            return Directory.GetFiles(GetFilePath(directoryPath)).ToList();
        }
    }
}

