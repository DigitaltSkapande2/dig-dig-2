using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using DigDig2.Debugging.Menu;

using UnityEngine;

namespace DigDig2.Debugging.Console
{
	[Debug]
	public abstract class ConsoleCommand : ScriptableObject
	{
		[SerializeField] private string commandWord = string.Empty;

		public string CommandWord
		{
			get => commandWord;
		}

		public virtual List<string> GetSuggestions( string[ ] args ) => new( )
		{
			""
		};

		public abstract UniTask<CommandResultContext> Process( string[ ] args );
	}
}
