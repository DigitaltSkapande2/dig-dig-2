using DigDig2.Debugging.Menu;

using UnityEngine;

namespace DigDig2.Debugging.Modules
{
	// Attribute to mark this class as "for Debug"
	[Debug]
	public class SampleDebugModule : MonoBehaviour
	{
		// private variable
		private int beans;

		// Will appear and be editable through the Debug Menu
		[DebugSerialized] private new string name = "walter white";
	}
}
