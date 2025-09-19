using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DigDig2.Debugging
{
    [Debug]
    public abstract class ConsoleCommandBase : ScriptableObject
    {
        [DebugSerialized] private string commandWord = string.Empty;

        public string CommandWord => commandWord;

        public virtual List<string> GetSuggestions(string[] args)
        {
            return new List<string>() { "" };
        }

        public abstract UniTask<CommandResultContext> Process(string[] args);
    }
}

