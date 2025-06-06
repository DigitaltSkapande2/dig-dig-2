using System.Collections.Generic;
using UnityEngine;

namespace DigDig2.Debug
{
	[CreateAssetMenu(fileName = "ChangeDebugNotesVisibility", menuName = "DeveloperConsole/Commands/ChangeDebugNotesVisibility")]
	public class ChangeDebugNotesVisibility : ConsoleCommand
	{
		public override bool Process(string[] args)
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
					return false;
			}

			return true;
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

	[CreateAssetMenu(fileName = "ChangeArchivedDebugNotesVisibility", menuName = "DeveloperConsole/Commands/ChangeArchivedDebugNotesVisibility")]
	public class ChangeArchivedDebugNotesVisibility : ConsoleCommand
	{
		public override bool Process(string[] args)
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
					return false;
			}

			return true;
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
