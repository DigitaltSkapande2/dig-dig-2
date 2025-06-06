
using System.Collections.Generic;

namespace DigDig2.Debug
{
    public interface IConsoleCommand
    {
        string CommandWord { get; }
        bool Process(string[] args);
        List<string> GetSuggestions(string[] args);
    }
}

