using UnityEngine;

namespace DigDig2.Input
{
	[CreateAssetMenu( fileName = "InputContext", menuName = "Input System/Input Context" )]
	public class InputContext : ScriptableObject
	{
		public override string ToString( ) => $"InputContext ({name})";
	}
}
