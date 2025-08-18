using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DigDig2.Debugging
{
	[CreateAssetMenu(fileName = "ChangeDebugNotesVisibility", menuName = "DeveloperConsole/Commands/ChangeDebugNotesVisibility")]
	public class ChangeDebugNotesVisibility : ConsoleCommandBase
	{
		public override async UniTask<CommandResultContext> Process(string[] args)
		{
			switch (args[0])
			{
				case "show":
					DebugNotesManager.Instance.ShowDebugNotes = true;
					break;
				case "hide":
					DebugNotesManager.Instance.ShowDebugNotes = false;
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
