
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mono.Cecil;
using NUnit.Framework;

namespace DigDig2.Debug
{
    public class CommandSystem
    {
        private readonly IEnumerable<ConsoleCommandBase> commands;

        public CommandSystem(IEnumerable<ConsoleCommandBase> commands)
        {
            this.commands = commands;
        }

        internal List<string> GetSuggestions(CommandDescriptor _inputCommand)
        {
            if (_inputCommand.HasArguments && _inputCommand.args[_inputCommand.args.Length - 1] != null) // if the command word is provided
            {
                UnityEngine.Debug.Log($"CommandSystem: Getting suggestions for command '{_inputCommand.commandWord}' with arguments: '{string.Join(", ", _inputCommand.args)}'");
                var commandSuggestions = GetCommandSuggestions(_inputCommand);
                if (commandSuggestions == default || commandSuggestions.Count == 0)
                {
                    return null;
                }
                return commandSuggestions
                    .Where(suggestion => suggestion.StartsWith(_inputCommand.args[_inputCommand.args.Length - 1], StringComparison.OrdinalIgnoreCase))
                    .OrderBy(suggestion => LevenshteinDistance(suggestion, _inputCommand.args[0]))
                    .ToList()
                ;
            }

            UnityEngine.Debug.Log($"Commands: {string.Join(", ", commands.Select(c => c.CommandWord))}");

            // Return list of all commands
            return commands
                .Where(command => command.CommandWord.StartsWith(_inputCommand.commandWord, StringComparison.OrdinalIgnoreCase))
                .OrderBy(command => LevenshteinDistance(command.CommandWord, _inputCommand.commandWord))
                .Select(command => command.CommandWord)
                .ToList()
            ;
        }


        private List<string> GetCommandSuggestions(CommandDescriptor _inputCommand)
        {
            string command = _inputCommand.commandWord;

            ConsoleCommandBase _command = commands.FirstOrDefault(c => c.CommandWord.Equals(_inputCommand.commandWord, StringComparison.OrdinalIgnoreCase));

            if (_command != default)
            {
                return _command.GetSuggestions(_inputCommand.args);
            }

            return default;
        }


        public async UniTask<CommandResultContext> ProcessCommand(CommandDescriptor _inputCommand)
        {
            var command = commands.FirstOrDefault(c => c.CommandWord.Equals(_inputCommand.commandWord, StringComparison.OrdinalIgnoreCase));
            if (command != default)
            {
                return await command.Process(_inputCommand.args);
            }
            return new CommandResultContext(CommandResultContext.Type.Error, $"Command '{_inputCommand.commandWord}' not found.");
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

    public struct CommandDescriptor
    {
        public string commandWord;
        public string[] args;
        public bool HasArguments => args != null && args.Length > 0;
        public bool IsValid => !string.IsNullOrWhiteSpace(commandWord);

        public CommandDescriptor(string commandString)
        {
            if (string.IsNullOrWhiteSpace(commandString))
            {
                commandWord = string.Empty;
                args = null;
                UnityEngine.Debug.LogWarning("CommandDescriptor: Trying to create CommandDescriptor of null or empty commandString.");
                return;
            }

            var parts = commandString.Split(new[] { ' ' }, StringSplitOptions.None);
            //UnityEngine.Debug.Log(parts.Length + " parts found in commandString: " + commandString);
            this.commandWord = parts[0];
            this.args = parts.Skip(1).ToArray();
        }

        public string[] GetFullCommand()
        {
            if (args == null || args.Length == 0)
            {
                return new[] { commandWord };
            }

            var fullCommand = new string[args.Length + 1];
            fullCommand[0] = commandWord;
            Array.Copy(args, 0, fullCommand, 1, args.Length);
            return fullCommand;
        }

        public override string ToString()
        {
            if (args == null || args.Length == 0)
                return commandWord;
            return $"{commandWord} {string.Join(" ", args)}";
        }

        public static implicit operator CommandDescriptor(string commandString)
        {
            return new CommandDescriptor(commandString);
        }

        public static implicit operator string(CommandDescriptor commandDescriptor)
        {
            return commandDescriptor.ToString();
        }

        
    }

    public struct CommandResultContext
    {
        public Type type;
        public enum Type
        {
            Success,
            Error,
            Warning
        }

        public string errorMessage;

        public CommandResultContext(Type type, string message)
        {
            this.type = type;
            this.errorMessage = message;
        }
        public CommandResultContext(Type type)
        {
            this.type = Type.Success;
            this.errorMessage = string.Empty;
        }

        public static CommandResultContext Success => new CommandResultContext(Type.Success);
        public static CommandResultContext InvalidArguments => new CommandResultContext(Type.Error, "Invalid arguments provided.");
    }
}
