using System.Collections.Generic;
using UnityEngine;


namespace DigDig2.Debug
{
    [CreateAssetMenu(fileName = "New SpawnItemCommand Command", menuName = "DeveloperConsole/Commands/SpawnItemCommand")]
    public class SpawnItemCommand : ConsoleCommand
    {
        [SerializeField] itemXNames[] items;
        [SerializeField] GameObject pickupPrefab;

        public override bool Process(string[] args)
        {
            if (args.Length != 1) return false;

            foreach (itemXNames item in items)
            {
                foreach (string name in item.names)
                {
                    if (string.Equals(args[0], name))
                    {

                        return true;
                    }
                }
            }
            return true;
        }


        [System.Serializable]
        public struct itemXNames
        {
            public List<string> names;
            public GameObject prefab;
        }

        public override List<string> GetSuggestions(string[] args)
        {
            List<string> result = new List<string>();
            foreach (itemXNames item in items)
            {
                result.Add(item.names[0]);
            }
            return result;
        }
    }
}

