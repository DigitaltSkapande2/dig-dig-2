using UnityEngine;
using Cysharp.Threading.Tasks;

namespace DigDig2.Debugging
{
	[CreateAssetMenu(fileName = "ManageDebugNote", menuName = "Developer Console/Commands/Manage Debug Note")]
	public class ManageDebugNote : ConsoleCommandBase
	{
		[SerializeField] private bool editNote = false;

		public override async UniTask<CommandResultContext> Process(string[] args)
		{
			if (!editNote) DebugNotesManager.Instance.StartNotePlacement();
			else DebugNotesManager.Instance.StartNoteEditing();

			return CommandResultContext.Success;
		}
	}
}
