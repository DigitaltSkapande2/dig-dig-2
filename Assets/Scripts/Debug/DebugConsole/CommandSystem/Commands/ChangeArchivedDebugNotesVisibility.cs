using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace DigDig2.Debugging
{
	[CreateAssetMenu(fileName = "ChangeArchivedDebugNotesVisibility", menuName = "DeveloperConsole/Commands/ChangeArchivedDebugNotesVisibility")]
	public class ChangeArchivedDebugNotesVisibility : ConsoleCommandBase
	{
		public override async UniTask<CommandResultContext> Process(string[] args)
		{
			switch (args[0])
			{
				case "show":
					DebugNotesManager.Instance.ShowArchivedDebugNotes = true;
					break;
				case "hide":
					DebugNotesManager.Instance.ShowArchivedDebugNotes = false;
					break;
				default:
					return CommandResultContext.InvalidArguments;
			}

			return CommandResultContext.Success;
		}

		public override List<string> GetSuggestions(string[] args)
        {
			if (args.Length > 1) return new List<string>();

            List<string> result = new()
			{
				"show",
				"hide"
			};

            return result;
        }
	}
}
