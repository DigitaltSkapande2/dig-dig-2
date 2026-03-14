using System.Collections.Generic;

using UnityEngine.InputSystem;

namespace DigDig2.Input
{
	public class InputPlayer
	{
		public readonly List<InputDevice> connectedDevices = new( );
		public string name;
		public bool active = true;

		public override string ToString( ) => $"InputPlayer ({name})";
	}
}
