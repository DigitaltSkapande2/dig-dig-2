using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using DigDig2.SaveSystem;

using UnityEngine;

namespace DigDig2.Debugging.Console.Commands
{
	[CreateAssetMenu( fileName = "ManageSaves", menuName = "Developer Console/Commands/Manage Saves" )]
	public class ManageSaves : ConsoleCommand
	{
		[SerializeField] private ManagementType managementType;

		public override async UniTask<CommandResultContext> Process( string[ ] args )
		{
			if ( args.Length < 1 ) return CommandResultContext.InvalidArguments;

			bool success = false;
			switch ( managementType )
			{
				case ManagementType.Save:
					SaveManager.Instance.SaveAllAndWriteToFile( args[ 0 ] );
					success = true;
					break;
				case ManagementType.Load: success = SaveManager.Instance.LoadSave( args[ 0 ] ); break;
				default: throw new ArgumentOutOfRangeException( );
			}

			await UniTask.WaitForEndOfFrame( );

			return success ? CommandResultContext.Success : CommandResultContext.InvalidArguments;
		}

		public override List<string> GetSuggestions( string[ ] args ) => SaveManager.Instance.GetSaveFileSaveNames( );

		private enum ManagementType { Save, Load }
	}
}
