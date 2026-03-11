using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using DigDig2.Debugging.Notes;

using UnityEngine;

namespace DigDig2.Debugging.Console.Commands
{
	[CreateAssetMenu( fileName = "ChangeDebugNotesVisibility", menuName = "Developer Console/Commands/Change Debug Notes Visibility" )]
	public class ChangeDebugNotesVisibility : ConsoleCommand
	{
		[SerializeField] private bool changeArchivedVisibility;

		public override async UniTask<CommandResultContext> Process( string[ ] args )
		{
			switch ( args[ 0 ] )
			{
				case "show":
					if ( !changeArchivedVisibility )
						DebugNotesManager.Instance.ShowDebugNotes = true;
					else
						DebugNotesManager.Instance.ShowArchivedDebugNotes = true;

					break;
				case "hide":
					if ( !changeArchivedVisibility )
						DebugNotesManager.Instance.ShowDebugNotes = false;
					else
						DebugNotesManager.Instance.ShowArchivedDebugNotes = false;

					break;
				default: return CommandResultContext.InvalidArguments;
			}

			await UniTask.WaitForEndOfFrame( );

			return CommandResultContext.Success;
		}

		public override List<string> GetSuggestions( string[ ] args )
		{
			if ( args.Length > 1 ) return new( );

			List<string> result = new( )
			{
				"show",
				"hide"
			};

			return result;
		}
	}
}
