using System;
using System.IO;
using IO.Swagger.Model;
using UnityEngine;

namespace DigDig2
{
    public static class FileSystem
    {
        public static void WriteDataToFile(object data, string fileName)
        {
            string filePath = GetFilePath(fileName + ".json");
            ValidateFilePath(filePath);

            string dataText = JsonUtility.ToJson(data);

            File.WriteAllText(filePath, dataText);
        }

        public static object ReadDataFromFile(string fileName)
        {
            string filePath = GetFilePath(fileName + ".json");
            if (IsFilePathValid(filePath))
            {
                Debug.LogError($"Trying To Read Nonexistent File {filePath}");
                return null;
            }
            string dataAsText = File.ReadAllText(fileName);

            return JsonUtility.FromJson<object>(dataAsText); // SERIALIZATION ABSTRACTION???
        }

        public static string GetFilePath(string fileName)
        {
            return Path.Join(UnityEngine.Application.persistentDataPath, fileName);
        }

        public static bool IsFilePathValid(string filePath)
        {
            if (!Path.IsPathFullyQualified(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            return true;
        }

        public static void ValidateFilePath(string filePath)
        {
            if (Directory.Exists(Path.GetDirectoryName(filePath)) && IsFilePathValid(filePath)) return;

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        }
    }
}

