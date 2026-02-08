using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DigDig2.Debugging
{
	[CreateAssetMenu(fileName = "ManageSaves", menuName = "Developer Console/Commands/Manage Saves")]
	public class ManageSaves : ConsoleCommandBase
	{
		[SerializeField] private ManagementType managementType;
		private enum ManagementType
		{
			Save,
			Load
		}

		public override async UniTask<CommandResultContext> Process(string[] args)
		{
			if (args.Length < 1) return CommandResultContext.InvalidArguments;

			bool success = false;
			switch (managementType)
            {
                case ManagementType.Save: SaveManager.Instance.SaveAllAndWriteToFile(args[0]); success = true; break;
                case ManagementType.Load: success = SaveManager.Instance.LoadSave(args[0]); break;
            }

			if (success) return CommandResultContext.Success;
            return CommandResultContext.InvalidArguments;
        }

        public override List<string> GetSuggestions(string[] args)
        {
            return SaveManager.Instance.GetSaveFileSaveNames();
        }
    }
}
