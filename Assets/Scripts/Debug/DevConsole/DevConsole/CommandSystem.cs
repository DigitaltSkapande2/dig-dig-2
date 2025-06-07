
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigDig2.Debug
{
    public class CommandSystem
    {
        private readonly IEnumerable<IConsoleCommand> commands;

        public CommandSystem(IEnumerable<IConsoleCommand> commands)
        {
            this.commands = commands;
        }

        internal List<string> GetSuggestions(string _inputText)
        {
            return commands
                .Where(command => command.CommandWord.StartsWith(_inputText, StringComparison.OrdinalIgnoreCase))
                .OrderBy(command => LevenshteinDistance(command.CommandWord, _inputText))
                .ThenBy(command => command.CommandWord.StartsWith(_inputText, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .Select(command => command.CommandWord)
                .ToList();
        }


        internal List<string> GetSpecificCommandSuggestionList(string command, string followingText)
        {
            IConsoleCommand commandObj = commands.FirstOrDefault(c => c.CommandWord.Equals(command, StringComparison.OrdinalIgnoreCase));
            if (commandObj != null)
            {
                string[] inputSplit = followingText.Split(' ');

                string commandInput = inputSplit[0];
                string[] args = inputSplit.Skip(1).ToArray();

                return commandObj.GetSuggestions(args)
                    .Where(suggestion => suggestion.StartsWith(followingText, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(suggestion => LevenshteinDistance(suggestion, followingText))
                    .ToList();
            }
            return new List<string>();
        }

        public bool ProcessCommand(string commandInput, string[] args)
        {
            foreach (var command in commands)
            {
                if (!commandInput.Equals(command.CommandWord, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (command.Process(args))
                {
                    return true;
                }
            }
            return false;
        }

        private List<string> FuzzySort(IEnumerable<string> items, string input)
        {
            input = input ?? string.Empty;
            return items
                .OrderBy(item =>
                {
                    if (item.Equals(input, StringComparison.OrdinalIgnoreCase)) return 0; // exact match
                    if (item.StartsWith(input, StringComparison.OrdinalIgnoreCase)) return 1; // prefix match
                    if (item.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0) return 2; // substring match
                    return 3; // fallback to distance
                })
                .ThenBy(item => LevenshteinDistance(item, input))
                .ThenBy(item => item, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static int LevenshteinDistance(string source, string target)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));

            int sourceLength = source.Length;
            int targetLength = target.Length;

            if (sourceLength == 0) return targetLength;
            if (targetLength == 0) return sourceLength;

            int[,] distance = new int[sourceLength + 1, targetLength + 1];

            for (int i = 0; i <= sourceLength; distance[i, 0] = i++) { }
            for (int j = 0; j <= targetLength; distance[0, j] = j++) { }

            for (int i = 1; i <= sourceLength; i++)
            {
                for (int j = 1; j <= targetLength; j++)
                {
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceLength, targetLength];
        }


    }
}
