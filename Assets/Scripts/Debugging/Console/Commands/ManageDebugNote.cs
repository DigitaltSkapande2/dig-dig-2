using Cysharp.Threading.Tasks;

using DigDig2.Debugging.Notes;

using UnityEngine;

namespace DigDig2.Debugging.Console.Commands {
	[CreateAssetMenu( fileName = "ManageDebugNote", menuName = "Developer Console/Commands/Manage Debug Note" )]
	public class ManageDebugNote : ConsoleCommand {
		[SerializeField] private bool editNote;

		public override async UniTask<CommandResultContext> Process( string[ ] args ) {
			if ( !editNote )
				DebugNotesManager.Instance.StartNotePlacement( );
			else
				DebugNotesManager.Instance.StartNoteEditing( );

			await UniTask.WaitForEndOfFrame( );

			return CommandResultContext.Success;
		}
	}
}
